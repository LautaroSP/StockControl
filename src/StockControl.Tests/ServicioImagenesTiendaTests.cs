using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioImagenesTiendaTests
{
    private const string Ean = "7790895000997";
    private static readonly DateTimeOffset Ahora = new(2026, 10, 26, 12, 0, 0, TimeSpan.FromHours(-3));
    private const string UrlOff = "https://images.openfoodfacts.org/images/products/779/089/500/0997/front_es.4.400.jpg";

    private static Producto Producto(int id = 1, int local = 1, string codigo = Ean, bool publicado = true, bool sector = false) => new()
    {
        Id = id, IdLocal = local, Codigo = codigo, Nombre = $"Producto {id}", Precio = 1000,
        Publicado = publicado, ProductoSector = sector
    };

    private static Local Tienda(string estado = ServicioTienda.Activa, int local = 1) => new()
    {
        IdLocal = local, Nombre = "Almagro", EstadoAbono = ServicioAbono.AlDia, Vence = Ahora.AddDays(20),
        AliasTienda = "almagro", EstadoTienda = estado
    };

    private static ImagenProducto Imagen(int orden, int id = 1, int producto = 1, int local = 1) => new()
    {
        Id = id, IdLocal = local, IdProducto = producto, Orden = orden, Archivo = $"{orden}.webp", TipoContenido = ServicioImagenesTienda.WebP
    };

    [Fact]
    public void Nombre_de_contenido_es_igual_para_la_misma_imagen_y_distinto_para_otra()
    {
        Assert.Equal(ServicioImagenesTienda.NombreDeContenido([1, 2, 3]), ServicioImagenesTienda.NombreDeContenido([1, 2, 3]));
        Assert.NotEqual(ServicioImagenesTienda.NombreDeContenido([1, 2, 3]), ServicioImagenesTienda.NombreDeContenido([1, 2, 4]));
    }

    [Fact]
    public void Nombre_de_url_es_estable_y_con_formato_de_archivo_del_almacen()
    {
        var nombre = ServicioImagenesTienda.NombreDeUrl(UrlOff);
        Assert.Equal(nombre, ServicioImagenesTienda.NombreDeUrl(UrlOff));
        Assert.Matches("^[0-9a-f]{64}\\.webp$", nombre);
    }

    [Fact]
    public void El_nombre_de_archivo_entra_en_la_columna()
    {
        Assert.True(ServicioImagenesTienda.NombreDeUrl(UrlOff).Length <= ServicioImagenesTienda.LargoMaximoArchivo);
        Assert.True(ServicioImagenesTienda.NombreDeContenido([1, 2, 3]).Length <= ServicioImagenesTienda.LargoMaximoArchivo);
    }

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0];
    private static readonly byte[] WebP = [(byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P'];

    [Fact]
    public void Propone_solo_con_codigo_de_barras()
    {
        Assert.True(ServicioImagenesTienda.PuedeProponer(Producto()));
        Assert.False(ServicioImagenesTienda.PuedeProponer(Producto(codigo: "7791")));
        Assert.False(ServicioImagenesTienda.PuedeProponer(Producto(codigo: "ABC-123456")));
    }

    [Fact]
    public void No_propone_si_el_producto_no_esta_publicado_o_es_sector()
    {
        Assert.False(ServicioImagenesTienda.PuedeProponer(Producto(publicado: false)));
        Assert.False(ServicioImagenesTienda.PuedeProponer(Producto(sector: true)));
    }

    [Fact]
    public void Aceptar_guarda_la_imagen_con_su_atribucion()
    {
        var imagen = ServicioImagenesTienda.NuevaDeOpenFoodFacts(Producto(), 1, "a.webp", ServicioImagenesTienda.WebP, Ahora);

        Assert.Equal(ServicioImagenesTienda.OrigenOpenFoodFacts, imagen.Origen);
        Assert.Equal("Open Food Facts", imagen.Fuente);
        Assert.Equal($"https://world.openfoodfacts.org/product/{Ean}", imagen.Enlace);
        Assert.Equal("CC-BY-SA", imagen.Licencia);
    }

    [Fact]
    public void Aceptar_sin_codigo_de_barras_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() =>
            ServicioImagenesTienda.NuevaDeOpenFoodFacts(Producto(codigo: "7791"), 1, "a.webp", ServicioImagenesTienda.WebP, Ahora));
    }

    [Fact]
    public void La_imagen_propia_no_lleva_atribucion()
    {
        var imagen = ServicioImagenesTienda.NuevaPropia(Producto(), 2, "a.webp", ServicioImagenesTienda.WebP, Ahora);

        Assert.Equal(ServicioImagenesTienda.OrigenPropia, imagen.Origen);
        Assert.Equal("", imagen.Fuente);
        Assert.Equal("", imagen.Licencia);
    }

    [Fact]
    public void No_se_cargan_imagenes_a_un_producto_sin_publicar()
    {
        Assert.Throws<ErrorNegocio>(() =>
            ServicioImagenesTienda.NuevaPropia(Producto(publicado: false), 1, "a.webp", ServicioImagenesTienda.WebP, Ahora));
    }

    [Fact]
    public void Rechazar_una_propuesta_no_se_vuelve_a_ofrecer()
    {
        var rechazadas = new List<ImagenRechazada>();
        var rechazo = ServicioImagenesTienda.Rechazar(Producto(), UrlOff, rechazadas);
        rechazadas.Add(rechazo!);

        Assert.NotNull(rechazo);
        Assert.True(ServicioImagenesTienda.EstaRechazada(UrlOff, rechazadas));
        Assert.Null(ServicioImagenesTienda.Rechazar(Producto(), UrlOff, rechazadas));
    }

    [Fact]
    public void Rechazar_con_una_direccion_ajena_no_guarda_nada()
    {
        Assert.Null(ServicioImagenesTienda.Rechazar(Producto(), "https://sitio-ajeno.com/foto.jpg", []));
    }

    [Fact]
    public void Una_imagen_nueva_en_un_lugar_ocupado_reemplaza_a_la_anterior()
    {
        var imagenes = new List<ImagenProducto> { Imagen(1, id: 1), Imagen(2, id: 2) };
        var nueva = Imagen(1, id: 3);

        var anterior = ServicioImagenesTienda.Colocar(imagenes, nueva);

        Assert.Equal(1, anterior!.Id);
        Assert.Equal(2, imagenes.Count);
        Assert.Contains(nueva, imagenes);
    }

    [Fact]
    public void El_tope_es_de_dos_imagenes_por_producto()
    {
        var imagenes = new List<ImagenProducto> { Imagen(1), Imagen(2) };

        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.Colocar(imagenes, Imagen(3)));
        Assert.Equal(2, imagenes.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void El_lugar_tiene_que_ser_1_o_2(int orden)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.ValidarOrden(orden));
    }

    [Fact]
    public void Reconoce_JPG_PNG_y_WebP_por_los_bytes()
    {
        Assert.Equal(ServicioImagenesTienda.Png, ServicioImagenesTienda.TipoReal(Png));
        Assert.Equal(ServicioImagenesTienda.Jpeg, ServicioImagenesTienda.TipoReal(Jpeg));
        Assert.Equal(ServicioImagenesTienda.WebP, ServicioImagenesTienda.TipoReal(WebP));
    }

    [Fact]
    public void Un_archivo_que_no_es_imagen_se_rechaza_aunque_se_llame_jpg()
    {
        var texto = System.Text.Encoding.UTF8.GetBytes("<html>no soy una imagen.jpg</html>");

        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.TipoReal(texto));
    }

    [Fact]
    public void Un_GIF_se_rechaza()
    {
        var gif = System.Text.Encoding.ASCII.GetBytes("GIF89a....");

        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.TipoReal(gif));
    }

    [Fact]
    public void Un_archivo_vacio_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.TipoReal([]));
    }

    [Fact]
    public void Una_imagen_de_mas_de_2_MB_se_rechaza()
    {
        var pesada = new byte[ServicioImagenesTienda.MaximoBytes + 1];
        Png.CopyTo(pesada, 0);

        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.TipoReal(pesada));
    }

    [Theory]
    [InlineData(800, 600, 400, 300)]
    [InlineData(600, 800, 300, 400)]
    [InlineData(1000, 2000, 200, 400)]
    [InlineData(400, 400, 400, 400)]
    [InlineData(300, 200, 300, 200)]
    [InlineData(4000, 10, 400, 1)]
    public void Entra_en_400_por_400_sin_deformarse_ni_agrandarse(int ancho, int alto, int esperadoAncho, int esperadoAlto)
    {
        Assert.Equal((esperadoAncho, esperadoAlto), ServicioImagenesTienda.Dimensiones(ancho, alto));
    }

    [Fact]
    public void Una_imagen_de_demasiados_pixeles_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.ValidarPixeles(10_000, 10_000));
        ServicioImagenesTienda.ValidarPixeles(4000, 3000);
    }

    [Fact]
    public void Decodifica_el_base64_con_o_sin_prefijo()
    {
        var base64 = Convert.ToBase64String(Png);

        Assert.Equal(Png, ServicioImagenesTienda.Decodificar(base64));
        Assert.Equal(Png, ServicioImagenesTienda.Decodificar($"data:image/png;base64,{base64}"));
    }

    [Fact]
    public void Un_base64_roto_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.Decodificar("###"));
    }

    [Fact]
    public void Un_texto_enorme_se_corta_antes_de_decodificar()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioImagenesTienda.Decodificar(new string('A', 4_000_000)));
    }

    [Theory]
    [InlineData("https://images.openfoodfacts.org/images/a.jpg", true)]
    [InlineData("https://images.openfoodfacts.net/images/a.jpg", true)]
    [InlineData("http://images.openfoodfacts.org/images/a.jpg", false)]
    [InlineData("https://images.openfoodfacts.org.malo.com/a.jpg", false)]
    [InlineData("https://malo.com/images.openfoodfacts.org/a.jpg", false)]
    [InlineData("https://images.openfoodfacts.org:8443/a.jpg", false)]
    [InlineData("https://127.0.0.1/a.jpg", false)]
    [InlineData("/images/a.jpg", false)]
    [InlineData("", false)]
    public void Solo_se_descarga_del_servidor_de_imagenes_de_Open_Food_Facts(string url, bool permitida)
    {
        Assert.Equal(permitida, ServicioImagenesTienda.UrlPermitida(url));
    }

    [Fact]
    public void La_imagen_se_sirve_con_el_producto_publicado_y_la_tienda_abierta()
    {
        Assert.True(ServicioImagenesTienda.Disponible(Imagen(1), Producto(), Tienda(), Ahora));
    }

    [Fact]
    public void La_imagen_de_un_producto_no_publicado_no_se_sirve()
    {
        Assert.False(ServicioImagenesTienda.Disponible(Imagen(1), Producto(publicado: false), Tienda(), Ahora));
    }

    [Fact]
    public void La_imagen_de_otro_local_no_se_sirve()
    {
        Assert.False(ServicioImagenesTienda.Disponible(Imagen(1, local: 2), Producto(local: 2), Tienda(local: 1), Ahora));
    }

    [Fact]
    public void La_imagen_de_otro_producto_no_se_sirve()
    {
        Assert.False(ServicioImagenesTienda.Disponible(Imagen(1, producto: 5), Producto(id: 1), Tienda(), Ahora));
    }

    [Theory]
    [InlineData(ServicioTienda.Pausada)]
    [InlineData(ServicioTienda.Inactiva)]
    public void La_imagen_no_se_sirve_con_la_tienda_pausada_o_inactiva(string estado)
    {
        Assert.False(ServicioImagenesTienda.Disponible(Imagen(1), Producto(), Tienda(estado), Ahora));
    }

    [Fact]
    public void El_producto_de_la_tienda_lleva_sus_imagenes_en_orden_sin_rutas()
    {
        var imagenes = new[] { Imagen(2, id: 20), Imagen(1, id: 10), Imagen(1, id: 99, producto: 7) };

        var producto = ServicioCatalogoTienda.AProductoTienda(Producto(), null, imagenes);

        Assert.Equal([10, 20], producto.Imagenes.Select(i => i.Id));
    }

    [Fact]
    public void Sin_imagenes_el_producto_de_la_tienda_queda_con_la_lista_vacia()
    {
        Assert.Empty(ServicioCatalogoTienda.AProductoTienda(Producto(), null).Imagenes);
    }
}
