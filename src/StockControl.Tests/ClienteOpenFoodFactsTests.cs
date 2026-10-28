using System.Net;
using StockControl.Api.Tienda;

namespace StockControl.Tests;

public class ClienteOpenFoodFactsTests
{
    private const string Ean = "7790895000997";
    private const string UrlEs = "https://images.openfoodfacts.org/images/products/779/front_es.4.400.jpg";
    private const string UrlEn = "https://images.openfoodfacts.org/images/products/779/front_en.5.400.jpg";

    private sealed class Falso : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responder;
        public int Llamadas { get; private set; }

        public Falso(Func<HttpResponseMessage> responder) => _responder = responder;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage solicitud, CancellationToken cancelar)
        {
            Llamadas++;
            return Task.FromResult(_responder());
        }
    }

    private static ClienteOpenFoodFacts Cliente(Func<HttpResponseMessage> responder) =>
        new(new HttpClient(new Falso(responder)) { BaseAddress = new Uri("https://world.openfoodfacts.org/") });

    private static HttpResponseMessage Json(string json, HttpStatusCode estado = HttpStatusCode.OK) =>
        new(estado) { Content = new StringContent(json) };

    private static string ConFrontal(string display) =>
        $$"""{ "code": "7790895000997", "product": { "selected_images": { "front": { "display": {{display}} } } } }""";

    [Fact]
    public void Lee_la_foto_frontal_en_espanol_si_existe()
    {
        var json = ConFrontal($$"""{ "en": "{{UrlEn}}", "es": "{{UrlEs}}" }""");

        Assert.Equal(UrlEs, ClienteOpenFoodFacts.LeerImagenFrontal(json));
    }

    [Fact]
    public void Sin_espanol_toma_el_primer_idioma_disponible()
    {
        var json = ConFrontal($$"""{ "en": "{{UrlEn}}" }""");

        Assert.Equal(UrlEn, ClienteOpenFoodFacts.LeerImagenFrontal(json));
    }

    [Fact]
    public void Sin_fotos_seleccionadas_usa_la_imagen_frontal_del_producto()
    {
        var json = $$"""{ "product": { "image_front_url": "{{UrlEs}}" } }""";

        Assert.Equal(UrlEs, ClienteOpenFoodFacts.LeerImagenFrontal(json));
    }

    [Fact]
    public void Un_producto_sin_foto_no_propone_nada()
    {
        Assert.Null(ClienteOpenFoodFacts.LeerImagenFrontal("""{ "product": { "product_name": "Galletitas" } }"""));
        Assert.Null(ClienteOpenFoodFacts.LeerImagenFrontal("""{ "status": "failure" }"""));
    }

    [Fact]
    public void Una_foto_de_otro_servidor_no_se_propone()
    {
        var json = ConFrontal("""{ "es": "https://sitio-ajeno.com/foto.jpg" }""");

        Assert.Null(ClienteOpenFoodFacts.LeerImagenFrontal(json));
    }

    [Fact]
    public async Task Consultar_devuelve_la_direccion_de_la_foto()
    {
        var cliente = Cliente(() => Json(ConFrontal($$"""{ "es": "{{UrlEs}}" }""")));

        var propuesta = await cliente.Consultar(Ean, CancellationToken.None);

        Assert.Equal(UrlEs, propuesta.Url);
        Assert.False(propuesta.SinRespuesta);
    }

    [Fact]
    public async Task Un_producto_que_Open_Food_Facts_no_conoce_no_es_una_caida()
    {
        var cliente = Cliente(() => Json("{}", HttpStatusCode.NotFound));

        var propuesta = await cliente.Consultar(Ean, CancellationToken.None);

        Assert.Null(propuesta.Url);
        Assert.False(propuesta.SinRespuesta);
    }

    [Fact]
    public async Task Si_la_fuente_cae_el_apartado_sigue_y_avisa_que_no_hubo_respuesta()
    {
        var cliente = Cliente(() => Json("error", HttpStatusCode.ServiceUnavailable));

        var propuesta = await cliente.Consultar(Ean, CancellationToken.None);

        Assert.Null(propuesta.Url);
        Assert.True(propuesta.SinRespuesta);
    }

    [Fact]
    public async Task Una_respuesta_que_no_es_json_no_rompe()
    {
        var cliente = Cliente(() => Json("<html>mantenimiento</html>"));

        var propuesta = await cliente.Consultar(Ean, CancellationToken.None);

        Assert.True(propuesta.SinRespuesta);
    }

    [Fact]
    public async Task Sin_codigo_de_barras_ni_siquiera_consulta()
    {
        var falso = new Falso(() => Json("{}"));
        var cliente = new ClienteOpenFoodFacts(new HttpClient(falso) { BaseAddress = new Uri("https://world.openfoodfacts.org/") });

        var propuesta = await cliente.Consultar("GENERIC-1", CancellationToken.None);

        Assert.Null(propuesta.Url);
        Assert.Equal(0, falso.Llamadas);
    }

    [Fact]
    public async Task No_descarga_de_un_servidor_que_no_es_Open_Food_Facts()
    {
        var falso = new Falso(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) });
        var cliente = new ClienteOpenFoodFacts(new HttpClient(falso));

        await Assert.ThrowsAsync<InvalidOperationException>(() => cliente.Descargar("http://127.0.0.1/secreto", CancellationToken.None));
        Assert.Equal(0, falso.Llamadas);
    }

    [Fact]
    public async Task No_descarga_una_imagen_de_mas_de_2_MB()
    {
        var enorme = new byte[2 * 1024 * 1024 + 10];
        var cliente = Cliente(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(enorme) });

        await Assert.ThrowsAsync<InvalidOperationException>(() => cliente.Descargar(UrlEs, CancellationToken.None));
    }

    [Fact]
    public async Task Descarga_una_imagen_chica()
    {
        var cliente = Cliente(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) });

        Assert.Equal([1, 2, 3], await cliente.Descargar(UrlEs, CancellationToken.None));
    }
}
