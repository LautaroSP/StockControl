namespace StockControl.Dominio;

public static class DiaArgentina
{
    public static TimeZoneInfo Zona { get; } = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Argentina Standard Time" : "America/Argentina/Buenos_Aires");

    public static DateOnly Hoy()
    {
        var ahora = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zona);
        return DateOnly.FromDateTime(ahora.DateTime);
    }

    public static (DateTimeOffset desde, DateTimeOffset hasta) Rango(DateOnly fecha)
    {
        var inicioLocal = fecha.ToDateTime(TimeOnly.MinValue);
        var finLocal = fecha.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var desde = new DateTimeOffset(inicioLocal, Zona.GetUtcOffset(inicioLocal)).ToUniversalTime();
        var hasta = new DateTimeOffset(finLocal, Zona.GetUtcOffset(finLocal)).ToUniversalTime();
        return (desde, hasta);
    }
}
