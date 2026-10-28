using StockControl.Api.Tienda;

namespace StockControl.Tests;

public class ClienteNominatimTests
{
    [Fact]
    public void Lee_nombre_latitud_y_longitud()
    {
        const string json = """
            [{"display_name":"Av. Rivadavia 1234, CABA","lat":"-34.6091234567","lon":"-58.3870001"}]
            """;

        var lugar = Assert.Single(ClienteNominatim.LeerLugares(json));

        Assert.Equal("Av. Rivadavia 1234, CABA", lugar.Nombre);
        Assert.Equal(-34.609123m, lugar.Latitud);
        Assert.Equal(-58.387000m, lugar.Longitud);
    }

    [Fact]
    public void Ignora_resultados_sin_coordenadas()
    {
        const string json = """[{"display_name":"Sin datos"},{"display_name":"Ok","lat":"-31.4","lon":"-64.18"}]""";

        Assert.Equal("Ok", Assert.Single(ClienteNominatim.LeerLugares(json)).Nombre);
    }

    [Fact]
    public void Respuesta_que_no_es_lista_no_trae_lugares()
    {
        Assert.Empty(ClienteNominatim.LeerLugares("""{"error":"x"}"""));
    }
}
