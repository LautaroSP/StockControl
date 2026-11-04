using StockControl.Api.Tienda;

namespace StockControl.Tests;

public class ClienteOpenRouteServiceTests
{
    [Fact]
    public void Lee_los_kilometros_del_recorrido()
    {
        const string json = """{"routes":[{"summary":{"distance":2500.4,"duration":300}}]}""";

        Assert.Equal(2.50m, ClienteOpenRouteService.LeerKm(json));
    }

    [Fact]
    public void Sin_recorrido_no_hay_kilometros()
    {
        Assert.Null(ClienteOpenRouteService.LeerKm("""{"routes":[]}"""));
        Assert.Null(ClienteOpenRouteService.LeerKm("""{"error":"no"}"""));
        Assert.Null(ClienteOpenRouteService.LeerKm("no es json"));
    }
}
