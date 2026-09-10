using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioTicketTests
{
    private static LineaVenta Linea(string nombre, decimal cant, decimal precio, decimal sub) =>
        new() { Codigo = "779", Nombre = nombre, Cantidad = cant, Precio = precio, SubTotal = sub };

    [Fact]
    public void POS_viejo_pasa_a_POS80()
    {
        Assert.Equal("POS-80", ServicioTicket.NormalizarFormato("POS"));
        Assert.Equal("POS-80", ServicioTicket.NormalizarFormato(""));
        Assert.Equal("POS-58", ServicioTicket.NormalizarFormato("pos-58"));
        Assert.Equal("A4", ServicioTicket.NormalizarFormato("A4"));
        Assert.Equal("POS-80", ServicioTicket.FormatoODefault("CARTA"));
    }

    [Fact]
    public void Formato_invalido_falla()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTicket.NormalizarFormato("CARTA"));
    }

    [Fact]
    public void Armar_incluye_local_lineas_y_total()
    {
        var t = ServicioTicket.Armar(
            "Lo de Pepe",
            DateTimeOffset.UtcNow,
            [Linea("Coca", 2, 1500, 3000)],
            3000,
            ["Efectivo"],
            0,
            false);

        Assert.Equal("Lo de Pepe", t.NombreLocal);
        Assert.Single(t.Lineas);
        Assert.Equal(2, t.Lineas[0].Cantidad);
        Assert.Equal(3000, t.Lineas[0].SubTotal);
        Assert.Equal(3000, t.Total);
        Assert.Contains("Efectivo", t.Medios);
        Assert.False(t.AlCosto);
        Assert.Equal(0, t.DescuentoPorcentaje);
    }

    [Fact]
    public void Armar_nota_descuento_y_al_costo()
    {
        var t = ServicioTicket.Armar(
            "Kiosco",
            DateTimeOffset.UtcNow,
            [Linea("Agua", 1, 800, 800)],
            800,
            ["Mercado Pago"],
            10,
            true);

        Assert.Equal(10, t.DescuentoPorcentaje);
        Assert.True(t.AlCosto);
    }

    [Fact]
    public void MediosDe_varios_pagos()
    {
        var medios = ServicioTicket.MediosDe(
            [
                new PagoVenta { DescripcionMetodoPago = "Efectivo", Importe = 60 },
                new PagoVenta { DescripcionMetodoPago = "Transferencia", Importe = 40 }
            ],
            "Efectivo, Transferencia");

        Assert.Equal(2, medios.Count);
        Assert.Contains(medios, m => m.StartsWith("Efectivo"));
        Assert.Contains(medios, m => m.StartsWith("Transferencia"));
    }

    [Fact]
    public void Armar_sin_items_falla()
    {
        Assert.Throws<ErrorNegocio>(() =>
            ServicioTicket.Armar("Local", DateTimeOffset.UtcNow, [], 0, ["Efectivo"], 0, false));
    }
}
