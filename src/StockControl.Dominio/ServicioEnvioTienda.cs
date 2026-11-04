namespace StockControl.Dominio;

public record DatosEnvio(string? Modalidad, string? Direccion, decimal? Latitud, decimal? Longitud, decimal? KmRecorrido);

public record EnvioCotizado(bool Disponible, decimal? DistanciaKm, decimal Importe, string? Origen, string? Motivo);

/// <summary>Precio del envío a partir de una distancia. La distancia por calle la trae la API; si no hay, se mide en línea recta.</summary>
public static class ServicioEnvioTienda
{
    public const string Retiro = "retiro";
    public const string Envio = "envio";
    public const string Recorrido = "recorrido";
    public const string Recta = "recta";

    public const decimal RadioTerrestreKm = 6371m;
    public const decimal RadioMaximoKm = 500m;

    public static bool Ofrece(Local local) =>
        local.HaceEnvios && ServicioTienda.MuestraMapa(local) && Configurado(local);

    /// <summary>La casilla ya está en el local. Con envío gratis siempre no hace falta el precio por km.</summary>
    public static void Asignar(Local local, bool gratisSiempre, decimal? precioKm, decimal? minimo, decimal? radioKm, decimal? gratisDesde)
    {
        var precio = Positivo(precioKm, "El precio por km");
        var piso = Positivo(minimo, "El envío mínimo");
        var radio = Positivo(radioKm, "El radio");
        if (radio > RadioMaximoKm)
            throw new ErrorNegocio($"El radio admite hasta {RadioMaximoKm:0} km.");
        if (local.HaceEnvios && !gratisSiempre && precio is null)
            throw new ErrorNegocio("Cargá el precio por km o marcá el envío gratis.");

        local.EnvioGratisSiempre = gratisSiempre;
        local.PrecioEnvioKm = precio;
        local.EnvioMinimo = piso;
        local.RadioEnvioKm = radio;
        local.EnvioGratisDesde = Positivo(gratisDesde, "El envío gratis desde");
    }

    /// <summary>kmRecorrido es null cuando OpenRouteService no aportó distancia.</summary>
    public static EnvioCotizado Cotizar(Local local, decimal subtotal, decimal? latitud, decimal? longitud, decimal? kmRecorrido)
    {
        if (!local.HaceEnvios || !Configurado(local))
            return Vacio("Este local no hace envíos.");
        if (!ServicioTienda.MuestraMapa(local))
            return Vacio("Este local no tiene ubicación para calcular el envío.");
        if (latitud is null || longitud is null)
            return Vacio("Marcá la dirección de entrega.");
        if (latitud is < -90 or > 90 || longitud is < -180 or > 180)
            return Vacio("La ubicación no es válida.");

        var (km, origen) = Distancia(local, latitud.Value, longitud.Value, kmRecorrido);
        if (local.RadioEnvioKm is { } radio && km > radio)
            return new EnvioCotizado(false, km, 0, origen, "Esa dirección está fuera de la zona de envío. Podés retirar en el local.");
        return new EnvioCotizado(true, km, Importe(local, subtotal, km), origen, null);
    }

    /// <summary>Retiro deja el envío en 0. Envío exige una cotización válida y una dirección.</summary>
    public static void Aplicar(PedidoTienda pedido, Local local, string? modalidad, string? direccion, decimal? latitud, decimal? longitud, decimal? kmRecorrido)
    {
        pedido.Modalidad = Retiro;
        pedido.DireccionEnvio = "";
        pedido.LatitudEnvio = null;
        pedido.LongitudEnvio = null;
        pedido.DistanciaKm = null;
        pedido.Envio = 0;
        pedido.OrigenDistancia = "";
        if (string.IsNullOrWhiteSpace(modalidad) || modalidad == Retiro)
            return;
        if (modalidad != Envio)
            throw new ErrorNegocio("La forma de entrega no es válida.");

        var cotizacion = Cotizar(local, pedido.Subtotal, latitud, longitud, kmRecorrido);
        if (!cotizacion.Disponible)
            throw new ErrorNegocio(cotizacion.Motivo!);
        var texto = (direccion ?? "").Trim();
        if (texto.Length < 4 || texto.Length > ServicioTienda.LargoMaximoDato)
            throw new ErrorNegocio("Escribí la dirección de entrega.");

        pedido.Modalidad = Envio;
        pedido.DireccionEnvio = texto;
        pedido.LatitudEnvio = Math.Round(latitud!.Value, 6);
        pedido.LongitudEnvio = Math.Round(longitud!.Value, 6);
        pedido.DistanciaKm = cotizacion.DistanciaKm;
        pedido.Envio = cotizacion.Importe;
        pedido.OrigenDistancia = cotizacion.Origen!;
    }

    /// <summary>Un grado de longitud en el ecuador son 111,19 km.</summary>
    public static decimal DistanciaEnLineaRecta(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        double Radians(decimal grados) => (double)grados * Math.PI / 180d;
        var dLat = Radians(lat2 - lat1);
        var dLon = Radians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var metros = (double)RadioTerrestreKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round((decimal)metros, 2, MidpointRounding.AwayFromZero);
    }

    private static bool Configurado(Local local) => local.EnvioGratisSiempre || local.PrecioEnvioKm is > 0;

    private static (decimal Km, string Origen) Distancia(Local local, decimal latitud, decimal longitud, decimal? kmRecorrido)
    {
        if (kmRecorrido is >= 0)
            return (Math.Round(kmRecorrido.Value, 2, MidpointRounding.AwayFromZero), Recorrido);
        return (DistanciaEnLineaRecta(local.Latitud!.Value, local.Longitud!.Value, latitud, longitud), Recta);
    }

    private static decimal Importe(Local local, decimal subtotal, decimal km)
    {
        if (local.EnvioGratisSiempre || local.EnvioGratisDesde is { } desde && subtotal >= desde)
            return 0;
        var importe = Math.Round(km * local.PrecioEnvioKm!.Value, 2, MidpointRounding.AwayFromZero);
        return local.EnvioMinimo is { } minimo && importe < minimo ? minimo : importe;
    }

    private static decimal? Positivo(decimal? valor, string campo)
    {
        if (valor is null) return null;
        var redondo = Math.Round(valor.Value, 2, MidpointRounding.AwayFromZero);
        if (redondo == 0) return null;
        if (redondo < 0)
            throw new ErrorNegocio($"{campo} tiene que ser mayor a 0.");
        return redondo;
    }

    private static EnvioCotizado Vacio(string motivo) => new(false, null, 0, null, motivo);
}
