using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPushTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(-3));
    private const string P256dh = "BNcRdreALRFXTkOOUHK1EtK2wtaz5Ry4YfYCA_0QTpQtUbVlUls0VJXg7A8u-Ts1XbjhazAkj7I99e8QcYP7DkM";
    private const string Auth = "tBHItJI5svbpez7KI4CCXg";

    [Theory]
    [InlineData("https://fcm.googleapis.com/fcm/send/abc")]
    [InlineData("https://updates.push.services.mozilla.com/wpush/v2/abc")]
    [InlineData("https://web.push.apple.com/abc")]
    [InlineData("https://wns2-bn3p.notify.windows.com/w/?token=abc")]
    public void Acepta_los_servicios_de_push_conocidos(string endpoint)
    {
        var suscripcion = ServicioPush.Crear(1, 2, endpoint, P256dh, Auth, Ahora);

        Assert.Equal(endpoint, suscripcion.Endpoint);
        Assert.Equal(1, suscripcion.IdLocal);
        Assert.Equal(2, suscripcion.IdUsuario);
    }

    [Theory]
    [InlineData("http://fcm.googleapis.com/fcm/send/abc")]
    [InlineData("https://127.0.0.1/admin")]
    [InlineData("https://localhost:5000/salud")]
    [InlineData("https://fcm.googleapis.com.atacante.com/x")]
    [InlineData("https://fcm.googleapis.com:8443/x")]
    [InlineData("https://atacantepush.apple.com/x")]
    [InlineData("")]
    public void Rechaza_endpoints_que_no_son_de_un_servicio_de_push(string endpoint)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPush.Crear(1, 2, endpoint, P256dh, Auth, Ahora));
    }

    [Theory]
    [InlineData("corta", Auth)]
    [InlineData(P256dh, "con espacios y signos!!")]
    public void Rechaza_claves_invalidas(string p256dh, string auth)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPush.Crear(1, 2, "https://fcm.googleapis.com/fcm/send/abc", p256dh, auth, Ahora));
    }
}
