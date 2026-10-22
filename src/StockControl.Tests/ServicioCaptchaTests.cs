using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCaptchaTests
{
    [Fact]
    public void Sin_las_dos_claves_el_captcha_no_esta_activo()
    {
        Assert.False(ServicioCaptcha.EstaActivo(null, null));
        Assert.False(ServicioCaptcha.EstaActivo("site", " "));
        Assert.False(ServicioCaptcha.EstaActivo("", "secreto"));
        Assert.True(ServicioCaptcha.EstaActivo("site", "secreto"));
    }

    [Fact]
    public void Sin_token_hay_que_confirmar_que_no_es_un_robot()
    {
        Assert.Equal("Confirmá que no sos un robot.", ServicioCaptcha.Falta(null));
        Assert.Equal("Confirmá que no sos un robot.", ServicioCaptcha.Falta("  "));
        Assert.Null(ServicioCaptcha.Falta("token"));
    }
}
