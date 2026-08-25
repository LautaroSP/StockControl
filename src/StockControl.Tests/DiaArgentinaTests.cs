using StockControl.Dominio;

namespace StockControl.Tests;

public class DiaArgentinaTests
{
    [Fact]
    public void Rango_medianoche_argentina_en_utc()
    {
        var (desde, hasta) = DiaArgentina.Rango(new DateOnly(2026, 8, 25));
        Assert.Equal(TimeSpan.Zero, desde.Offset);
        Assert.Equal(TimeSpan.Zero, hasta.Offset);
        Assert.Equal(new DateTimeOffset(2026, 8, 25, 3, 0, 0, TimeSpan.Zero), desde);
        Assert.Equal(new DateTimeOffset(2026, 8, 26, 3, 0, 0, TimeSpan.Zero), hasta);
    }
}
