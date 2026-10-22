using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockControl.Api.Auth;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Api.Importacion;
using StockControl.Api.Pagos;
using StockControl.Dominio;

CargarEnvLocal();

var importar = args.Length >= 2 && args[0] == "import";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<SesionActual>();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:Default")));
builder.Services.AddSingleton<JwtServicio>();
builder.Services.AddSingleton<PasswordHasher<Usuario>>();
var proteccion = builder.Services.AddDataProtection();
var rutaClaves = builder.Configuration["DataProtection:Ruta"];
if (!string.IsNullOrWhiteSpace(rutaClaves))
    proteccion.PersistKeysToFileSystem(new DirectoryInfo(rutaClaves));
builder.Services.AddHttpClient<ClienteMercadoPago>(c =>
{
    c.BaseAddress = new Uri("https://api.mercadopago.com/");
    c.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient<ClienteMercadoPagoLocal>(c =>
{
    c.BaseAddress = new Uri("https://api.mercadopago.com/");
    c.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHttpClient("turnstile", c =>
{
    c.BaseAddress = new Uri("https://challenges.cloudflare.com/");
    c.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient("resend", c =>
{
    c.BaseAddress = new Uri("https://api.resend.com/");
    c.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var clave = builder.Configuration["Jwt:Clave"]
                    ?? throw new InvalidOperationException("Falta Jwt:Clave");
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        o.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("api", c =>
    {
        c.PermitLimit = 120;
        c.Window = TimeSpan.FromMinutes(1);
        c.QueueLimit = 0;
    });
    o.AddPolicy("registro", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    o.AddPolicy("contacto", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (importar)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var ruta = args[1];
    var nombre = args.Length > 2 ? args[2] : "Local importado";
    var usuario = args.Length > 3 ? args[3] : "dueno";
    var clave = Environment.GetEnvironmentVariable("IMPORT_CLAVE_DUENO")
                ?? builder.Configuration["IMPORT_CLAVE_DUENO"]
                ?? throw new InvalidOperationException("Falta IMPORT_CLAVE_DUENO");
    var id = await ImportadorSqlite.Importar(db, ruta, nombre, usuario, clave);
    Console.WriteLine($"Importado en local {id}");
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var claveSeed = builder.Configuration["SEED_CLAVE"] ?? "cambiar-seed";
    await SemillaDesarrollo.Ejecutar(db, claveSeed);
    await AsegurarTelefonoVentas(db, builder.Configuration["Ventas:WhatsApp"]);
    await AsegurarEmailVentas(db);
}

app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Path.StartsWithSegments("/salud"))
    {
        await next();
        return;
    }

    ctx.Response.StatusCode = StatusCodes.Status200OK;
    ctx.Response.ContentType = "text/plain; charset=utf-8";
    if (!HttpMethods.IsHead(ctx.Request.Method))
        await ctx.Response.WriteAsync("ok");
});

app.UseCors();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<MiddlewareSesion>();
app.UseRateLimiter();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "same-origin";
    if (!ctx.Request.Host.Host.Contains("localhost"))
        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000";
    await next();
});

const int maxPagina = 100;
const string ErrorMercadoPago = "No se pudo iniciar el pago. Probá de nuevo o contactá a ventas.";

app.MapPost("/auth/login", async (LoginPedido pedido, AppDbContext db, PasswordHasher<Usuario> hasher, JwtServicio jwt) =>
{
    if (string.IsNullOrWhiteSpace(pedido.Usuario) || string.IsNullOrWhiteSpace(pedido.Clave))
        return Results.BadRequest("Usuario y clave son obligatorios.");
    var u = await db.Usuarios.IgnoreQueryFilters()
        .FirstOrDefaultAsync(x => x.NombreUsuario == pedido.Usuario);
    if (u == null || !u.Activo || hasher.VerifyHashedPassword(u, u.HashClave, pedido.Clave) == PasswordVerificationResult.Failed)
        return Results.Unauthorized();
    if (!u.EmailConfirmado)
        return Results.BadRequest("Confirmá el mail antes de entrar. Revisá la bandeja.");
    return Results.Ok(new { token = jwt.Emitir(u, null), rol = u.Rol, nombre = string.IsNullOrWhiteSpace(u.Nombre) ? u.NombreUsuario : u.Nombre });
}).RequireRateLimiting("api");

app.MapGet("/registro/captcha", (IConfiguration config, IWebHostEnvironment env) =>
{
    var claves = ClavesCaptcha(config, env);
    return Results.Ok(new { siteKey = claves?.SiteKey });
}).RequireRateLimiting("api");

app.MapPost("/registro", async (RegistroCuentaPedido pedido, HttpContext http, AppDbContext db, PasswordHasher<Usuario> hasher, JwtServicio jwt, IConfiguration config, IWebHostEnvironment env, IHttpClientFactory clientes) =>
{
    if (!string.IsNullOrEmpty(pedido.Sitio)) return Results.Ok();
    if (ClavesCaptcha(config, env) is { } captcha)
    {
        var falta = ServicioCaptcha.Falta(pedido.Captcha);
        if (falta != null) return Results.BadRequest(falta);
        var ok = await CaptchaValido(clientes, captcha.Secreto, pedido.Captcha!, http.Connection.RemoteIpAddress?.ToString());
        if (!ok) return Results.BadRequest("No pudimos comprobar el captcha. Probá de nuevo.");
    }
    try
    {
        ServicioRegistro.Validar(pedido, pedido.Clave);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    var (dueno, local) = ServicioRegistro.Armar(pedido, DateTimeOffset.UtcNow);
    if (await db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.NombreUsuario == dueno.NombreUsuario || u.Email == dueno.Email))
        return Results.Conflict("Ya existe una cuenta con ese usuario o mail.");
    dueno.HashClave = hasher.HashPassword(dueno, pedido.Clave);
    var apiKey = config["Correo:ApiKey"];
    var desde = config["Correo:Desde"];
    var urlWeb = config["App:UrlWeb"];
    var confirmar = ServicioConfirmacionCorreo.HayQueConfirmar(apiKey, desde, urlWeb);
    string? tokenPlano = null;
    if (confirmar)
    {
        tokenPlano = ServicioConfirmacionCorreo.NuevoToken();
        dueno.EmailConfirmado = false;
        dueno.TokenConfirmacion = ServicioConfirmacionCorreo.Hash(tokenPlano);
        dueno.VenceConfirmacion = DateTimeOffset.UtcNow.AddHours(ServicioConfirmacionCorreo.HorasDeVigencia);
    }

    await using var transaccion = await db.Database.BeginTransactionAsync();
    db.Locales.Add(local);
    db.Usuarios.Add(dueno);
    await db.SaveChangesAsync();
    db.UsuarioLocales.Add(new UsuarioLocal { IdUsuario = dueno.Id, IdLocal = local.IdLocal });
    db.MetodosPago.AddRange(ServicioRegistro.MediosIniciales.Select(m => new MetodoPago { IdLocal = local.IdLocal, Descripcion = m }));
    await db.SaveChangesAsync();
    if (confirmar)
    {
        var enlace = urlWeb!.TrimEnd('/') + "/confirmar?token=" + tokenPlano;
        if (!await EnviarConfirmacion(clientes, apiKey!, desde!, dueno.Email, enlace))
        {
            await transaccion.RollbackAsync();
            return Results.BadRequest("No pudimos enviar el mail de confirmación. Probá de nuevo.");
        }
    }
    await transaccion.CommitAsync();
    if (confirmar)
        return Results.Ok(new { pendiente = true });
    return Results.Ok(new { token = jwt.Emitir(dueno, null), rol = dueno.Rol, nombre = dueno.Nombre });
}).RequireRateLimiting("registro");

app.MapPost("/registro/confirmar", async (ConfirmarCorreoPedido pedido, AppDbContext db) =>
{
    var hash = ServicioConfirmacionCorreo.Hash(pedido.Token ?? "");
    var usuario = await db.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TokenConfirmacion == hash);
    var rechazo = ServicioConfirmacionCorreo.Rechazo(pedido.Token, usuario?.TokenConfirmacion, usuario?.VenceConfirmacion, DateTimeOffset.UtcNow);
    if (usuario == null || rechazo != null)
        return Results.BadRequest(rechazo ?? "El enlace no sirve. Registrate de nuevo.");
    ServicioConfirmacionCorreo.Confirmar(usuario);
    await db.SaveChangesAsync();
    return Results.Ok(new { confirmado = true });
}).RequireRateLimiting("registro");

app.MapGet("/locales", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    var ids = await db.UsuarioLocales.Where(x => x.IdUsuario == sesion.IdUsuario).Select(x => x.IdLocal).ToListAsync();
    var locales = await db.Locales.Where(l => ids.Contains(l.IdLocal) && l.Activo).OrderBy(l => l.Nombre).ToListAsync();
    var ahora = DateTimeOffset.UtcNow;
    return Results.Ok(new
    {
        totalMensual = ServicioAbono.TotalMensual(locales.Count(l => l.EstadoAbono != ServicioAbono.Pendiente)),
        precioMensual = ServicioAbono.PrecioMensual,
        items = locales.Select(l =>
        {
            var abono = ServicioAbono.Estado(l, ahora);
            return new { l.IdLocal, l.Nombre, abono.Estado, abono.Vence, abono.DiasRestantes, abono.PuedeOperarCaja, debito = l.EstadoSuscripcion };
        })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/locales/contratar", async (ContratarLocalPedido pedido, AppDbContext db, SesionActual sesion, ClienteMercadoPago mp, ILoggerFactory logs, IConfiguration config) =>
{
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    if (sesion.Rol != Roles.Dueno) return Results.Forbid();
    try
    {
        var local = ServicioAdmin.ContratarLocal(pedido.Nombre);
        await using var transaccion = await db.Database.BeginTransactionAsync();
        db.Locales.Add(local);
        await db.SaveChangesAsync();
        db.UsuarioLocales.Add(new UsuarioLocal { IdUsuario = sesion.IdUsuario.Value, IdLocal = local.IdLocal });
        db.MetodosPago.AddRange(ServicioRegistro.MediosIniciales.Select(m => new MetodoPago { IdLocal = local.IdLocal, Descripcion = m }));
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return await EmitirPagoContratacion(local, db, mp, logs, config);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/locales/{id:int}/contratar/pago", async (int id, AppDbContext db, SesionActual sesion, ClienteMercadoPago mp, ILoggerFactory logs, IConfiguration config) =>
{
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    if (sesion.Rol != Roles.Dueno) return Results.Forbid();
    var propio = await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == sesion.IdUsuario && x.IdLocal == id);
    if (!propio) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == id && l.Activo);
    if (local == null) return Results.NotFound();
    if (local.EstadoAbono != ServicioAbono.Pendiente)
        return Results.BadRequest("Ese local ya no está esperando el pago.");
    return await EmitirPagoContratacion(local, db, mp, logs, config);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/locales/actual/abono", async (AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == sesion.IdLocal);
    if (local == null) return Results.NotFound();
    var ahora = DateTimeOffset.UtcNow;
    var abono = ServicioAbono.Estado(local, ahora);
    var pagoPendiente = await AbonoPendiente(db, ahora) != null;
    var email = await db.Usuarios.IgnoreQueryFilters().Where(u => u.Id == sesion.IdUsuario).Select(u => u.Email).FirstOrDefaultAsync();
    return Results.Ok(new
    {
        abono.Estado,
        abono.Vence,
        abono.Limite,
        abono.DiasRestantes,
        abono.PuedeOperarCaja,
        abono.EnGracia,
        pagoPendiente,
        debito = local.EstadoSuscripcion,
        email = email ?? "",
        precioMensual = ServicioAbono.PrecioMensual,
        ventasWhatsApp = UrlContacto(await TelefonoVentas(db), $"Hola, quiero reactivar el local {local.Nombre}.")
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/locales/actual/abono/pagar", async (AppDbContext db, SesionActual sesion, ClienteMercadoPago mp, ILoggerFactory logs) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    try
    {
        ServicioAbono.ValidarPagoManual(local);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    var ahora = DateTimeOffset.UtcNow;
    var pendiente = await AbonoPendiente(db, ahora);
    if (pendiente != null) return Results.Ok(new { urlPago = pendiente.UrlPago });
    var abono = ServicioAbono.NuevoAbono(local.IdLocal, Abono.OrigenMp, ahora);
    db.Abonos.Add(abono);
    await db.SaveChangesAsync();
    try
    {
        var preferencia = await mp.CrearPreferencia(abono, local.Nombre);
        abono.IdPreferenciaMp = preferencia.Id;
        abono.UrlPago = preferencia.UrlPago;
        await db.SaveChangesAsync();
        return Results.Ok(new { urlPago = abono.UrlPago });
    }
    catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
    {
        logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo crear la preferencia del abono {IdAbono}", abono.Id);
        abono.Estado = Abono.Rechazado;
        await db.SaveChangesAsync();
        return Results.Json(ErrorMercadoPago, statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/locales/actual/abono/debito", async (DebitoPedido pedido, AppDbContext db, SesionActual sesion, ClienteMercadoPago mp, ILoggerFactory logs) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    var mail = pedido.MailPagador?.Trim().ToLowerInvariant() ?? "";
    if (mail.Length > 200 || !ServicioRegistro.EsMail(mail)) return Results.BadRequest("El mail de Mercado Pago no es válido.");
    var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    if (local.EstadoSuscripcion == ServicioAbono.DebitoActivo) return Results.Conflict("El débito automático ya está activo.");
    try
    {
        if (local.IdSuscripcionMp != null && local.EstadoSuscripcion is ServicioAbono.DebitoPendiente or ServicioAbono.DebitoPausado)
            await mp.CancelarSuscripcion(local.IdSuscripcionMp);
        var suscripcion = await mp.CrearSuscripcion(local, mail, ServicioAbono.InicioDebito(local, DateTimeOffset.UtcNow));
        local.IdSuscripcionMp = suscripcion.Id;
        local.EstadoSuscripcion = ServicioAbono.DebitoPendiente;
        await db.SaveChangesAsync();
        return Results.Ok(new { urlPago = suscripcion.UrlPago });
    }
    catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
    {
        logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo crear la suscripción del local {IdLocal}", local.IdLocal);
        return Results.Json(ErrorMercadoPago, statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapDelete("/locales/actual/abono/debito", async (AppDbContext db, SesionActual sesion, ClienteMercadoPago mp, ILoggerFactory logs) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    if (local.IdSuscripcionMp == null || local.EstadoSuscripcion is ServicioAbono.SinDebito or ServicioAbono.DebitoCancelado)
        return Results.BadRequest("No hay débito automático para cancelar.");
    try
    {
        await mp.CancelarSuscripcion(local.IdSuscripcionMp);
        local.EstadoSuscripcion = ServicioAbono.DebitoCancelado;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
    {
        logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo cancelar la suscripción del local {IdLocal}", local.IdLocal);
        return Results.Json("No se pudo cancelar el débito. Probá de nuevo.", statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/mercadopago/webhook", async (HttpRequest req, AppDbContext db, ClienteMercadoPago mp, ClienteMercadoPagoLocal mpLocal, IDataProtectionProvider proteccion, IConfiguration config, ILoggerFactory logs) =>
{
    var log = logs.CreateLogger("MercadoPago");
    string? tipo = req.Query["type"], id = req.Query["data.id"];
    if (string.IsNullOrEmpty(tipo)) tipo = req.Query["topic"];
    if (string.IsNullOrEmpty(id)) id = req.Query["id"];
    if (string.IsNullOrEmpty(tipo) || string.IsNullOrEmpty(id))
    {
        try
        {
            using var cuerpo = await JsonDocument.ParseAsync(req.Body);
            var raiz = cuerpo.RootElement;
            if (string.IsNullOrEmpty(tipo) && raiz.TryGetProperty("type", out var t)) tipo = t.ToString();
            if (string.IsNullOrEmpty(id) && raiz.TryGetProperty("data", out var d) && d.TryGetProperty("id", out var i)) id = i.ToString();
        }
        catch (JsonException)
        {
            return Results.BadRequest();
        }
    }
    if (!FirmaWebhook.EsValida(req.Headers["x-signature"], req.Headers["x-request-id"], id, config["MercadoPago:WebhookSecret"] ?? ""))
        return Results.Unauthorized();
    if (string.IsNullOrEmpty(id) || id.Length > 64) return Results.Ok();
    try
    {
        if (tipo == "order")
        {
            await CobrosMercadoPagoApp.AplicarAviso(db, mpLocal, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config, id);
            return Results.Ok();
        }
        await AvisosMercadoPago.Procesar(tipo, id, db, mp);
        return Results.Ok();
    }
    catch (ErrorNegocio ex)
    {
        log.LogWarning("Aviso {Tipo} {Id} ignorado: {Motivo}", tipo, id, ex.Message);
        return Results.Ok();
    }
    catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
    {
        log.LogWarning("Aviso {Tipo} {Id} ya procesado", tipo, id);
        return Results.Ok();
    }
}).RequireRateLimiting("api");

app.MapPost("/locales/{id:int}/entrar", async (int id, AppDbContext db, SesionActual sesion, JwtServicio jwt) =>
{
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    var ok = await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == sesion.IdUsuario && x.IdLocal == id);
    if (!ok) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == id);
    if (local == null || !local.Activo) return Results.BadRequest("Ese local está desactivado.");
    if (local.EstadoAbono == ServicioAbono.Pendiente)
        return Results.BadRequest("Este local se habilita cuando se acredite el pago.");
    var u = await db.Usuarios.IgnoreQueryFilters().FirstAsync(x => x.Id == sesion.IdUsuario);
    return Results.Ok(new { token = jwt.Emitir(u, id) });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/productos", async (AppDbContext db, SesionActual sesion, string? q, string? codigo, bool? sector, string? tipo, int? idGrupo, bool? sinGrupo, bool? stockBajo, int? idProveedor, int pagina = 1, int tamano = 50) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    tamano = Math.Clamp(tamano, 1, maxPagina);
    pagina = Math.Max(1, pagina);
    var consulta = db.Productos.AsQueryable();
    var soloSector = sector == true || string.Equals(tipo, "sector", StringComparison.OrdinalIgnoreCase);
    var soloComun = sector == false || string.Equals(tipo, "comun", StringComparison.OrdinalIgnoreCase);
    if (soloSector)
        consulta = consulta.Where(p => p.ProductoSector);
    else if (soloComun)
        consulta = consulta.Where(p => !p.ProductoSector);
    if (idGrupo is > 0)
        consulta = consulta.Where(p => p.IdGrupoProducto == idGrupo);
    else if (sinGrupo == true)
        consulta = consulta.Where(p => p.IdGrupoProducto == 0);
    if (idProveedor is > 0)
        consulta = consulta.Where(p => p.IdProveedor == idProveedor);
    var valorUmbral = (await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == "UmbralStockBajo"))?.Valor;
    var umbral = decimal.TryParse(valorUmbral, out var umbralConfigurado) && umbralConfigurado >= 0 ? umbralConfigurado : 5;
    var permisoPrecio = (await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == "EmpleadoPuedeModificarPrecios"))?.Valor == "1";
    if (stockBajo == true)
    {
        consulta = consulta.Where(p => !p.ProductoSector && p.Cantidad <= umbral);
    }
    if (!string.IsNullOrWhiteSpace(codigo))
        consulta = consulta.Where(p => p.Codigo == codigo);
    else if (!string.IsNullOrWhiteSpace(q))
        consulta = consulta.Where(p => EF.Functions.ILike(p.Nombre, $"%{q}%") || EF.Functions.ILike(p.Codigo, $"%{q}%"));
    consulta = consulta.OrderBy(p => p.Nombre);
    var total = await consulta.CountAsync();
    var items = await consulta.Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
    var nombresGrupo = await db.GrupoProductos.ToDictionaryAsync(g => g.IdGrupoProducto, g => g.NombreGrupo);
    return Results.Ok(new
    {
        total,
        pagina,
        tamano,
        umbralStockBajo = umbral,
        puedeModificarPrecio = Permisos.EsDuenoOperativo(sesion.Rol ?? "") || Permisos.PuedeModificarPrecioEmpleado(sesion.Rol ?? "", permisoPrecio),
        items = items.Select(p => new
        {
            p.Id,
            p.IdLocal,
            p.Codigo,
            p.Nombre,
            p.Cantidad,
            p.Costo,
            p.Precio,
            p.ProductoSector,
            p.IdGrupoProducto,
            p.FechaModificacion,
            p.UsuarioModificacion,
            nombreGrupo = p.IdGrupoProducto == 0
                ? null
                : nombresGrupo.GetValueOrDefault(p.IdGrupoProducto)
        })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/productos", async (ProductoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeCrearProducto(sesion.Rol ?? "")) return Results.Forbid();
    if (string.IsNullOrWhiteSpace(pedido.Codigo) || string.IsNullOrWhiteSpace(pedido.Nombre))
        return Results.BadRequest("Código y nombre son obligatorios.");
    if (await db.Productos.AnyAsync(p => p.Codigo == pedido.Codigo))
        return Results.Conflict("Ya existe un producto con ese código.");
    var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    var config = await LeerConfiguracionLocal(db, local);
    var tieneGrupo = pedido.IdGrupoProducto is int idGrupo && idGrupo > 0;
    var p = new Producto
    {
        IdLocal = sesion.IdLocal.Value,
        Codigo = pedido.Codigo,
        Nombre = pedido.Nombre,
        Cantidad = pedido.Cantidad,
        Costo = pedido.Costo,
        Precio = PrecioVenta.PrecioAlCrear(pedido.Costo, pedido.Precio, pedido.ProductoSector, tieneGrupo, config.FactorGanancia, config.Iva),
        ProductoSector = pedido.ProductoSector,
        FechaModificacion = DateTimeOffset.UtcNow,
        UsuarioModificacion = sesion.NombreUsuario ?? "sistema"
    };
    try
    {
        if (pedido.IdGrupoProducto is int idG && idG > 0)
        {
            var g = await db.GrupoProductos.FirstOrDefaultAsync(x => x.IdGrupoProducto == idG);
            if (g == null) return Results.BadRequest("El grupo no existe.");
            ServicioGrupo.Asignar(p, g, sesion.NombreUsuario ?? "sistema");
        }
        db.Productos.Add(p);
        await db.SaveChangesAsync();
        return Results.Created($"/productos/{p.Id}", p);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/productos/{id:int}", async (int id, ProductoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeEditarProducto(sesion.Rol ?? "")) return Results.Forbid();
    var p = await db.Productos.FirstOrDefaultAsync(x => x.Id == id);
    if (p == null) return Results.NotFound();
    p.Nombre = pedido.Nombre;
    p.Cantidad = pedido.Cantidad;
    p.Costo = pedido.Costo;
    p.Precio = pedido.Precio;
    p.ProductoSector = pedido.ProductoSector;
    p.FechaModificacion = DateTimeOffset.UtcNow;
    p.UsuarioModificacion = sesion.NombreUsuario ?? "sistema";
    try
    {
        if (pedido.IdGrupoProducto is int idG)
        {
            if (idG == 0)
                ServicioGrupo.Sacar(p, sesion.NombreUsuario ?? "sistema");
            else if (idG != p.IdGrupoProducto)
            {
                var g = await db.GrupoProductos.FirstOrDefaultAsync(x => x.IdGrupoProducto == idG);
                if (g == null) return Results.BadRequest("El grupo no existe.");
                ServicioGrupo.Asignar(p, g, sesion.NombreUsuario ?? "sistema");
            }
        }
        await db.SaveChangesAsync();
        return Results.Ok(p);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/productos/{id:int}/precio", async (int id, PrecioProductoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == id);
    if (producto == null) return Results.NotFound();
    var esEmpleado = sesion.Rol == Roles.Empleado;
    var habilitado = (await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == "EmpleadoPuedeModificarPrecios"))?.Valor == "1";
    if (!Permisos.EsDuenoOperativo(sesion.Rol ?? "") && !Permisos.PuedeModificarPrecioEmpleado(sesion.Rol ?? "", habilitado))
        return Results.Forbid();
    if (esEmpleado && producto.ProductoSector)
        return Results.BadRequest("El precio de un producto sector se define en caja.");
    if (esEmpleado && producto.IdGrupoProducto != 0)
        return Results.BadRequest("El precio de un producto de grupo se define desde el grupo.");
    if (pedido.Precio < 0) return Results.BadRequest("El precio no puede ser negativo.");
    producto.Precio = pedido.Precio;
    producto.FechaModificacion = DateTimeOffset.UtcNow;
    producto.UsuarioModificacion = sesion.NombreUsuario ?? "sistema";
    await db.SaveChangesAsync();
    return Results.Ok(producto);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/productos/{id:int}/stock", async (int id, StockPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (pedido.Cantidad <= 0) return Results.BadRequest("La cantidad tiene que ser mayor a 0.");
    var p = await db.Productos.FirstOrDefaultAsync(x => x.Id == id);
    if (p == null) return Results.NotFound();
    p.Cantidad += pedido.Cantidad;
    p.FechaModificacion = DateTimeOffset.UtcNow;
    p.UsuarioModificacion = sesion.NombreUsuario ?? "sistema";
    await db.SaveChangesAsync();
    return Results.Ok(p);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/grupos", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarGrupos(sesion.Rol ?? "")) return Results.Forbid();
    var grupos = await db.GrupoProductos.OrderBy(g => g.NombreGrupo).ToListAsync();
    var conteos = await db.Productos.Where(p => p.IdGrupoProducto != 0)
        .GroupBy(p => p.IdGrupoProducto)
        .Select(g => new { Id = g.Key, Cantidad = g.Count() })
        .ToDictionaryAsync(x => x.Id, x => x.Cantidad);
    return Results.Ok(grupos.Select(g => new
    {
        g.IdGrupoProducto,
        g.NombreGrupo,
        g.Costo,
        g.PrecioGrupo,
        g.Ganancia,
        g.GananciaIndividual,
        cantidad = conteos.GetValueOrDefault(g.IdGrupoProducto)
    }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/grupos/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarGrupos(sesion.Rol ?? "")) return Results.Forbid();
    var g = await db.GrupoProductos.FirstOrDefaultAsync(x => x.IdGrupoProducto == id);
    if (g == null) return Results.NotFound();
    var miembros = await db.Productos.Where(p => p.IdGrupoProducto == id)
        .OrderBy(p => p.Nombre)
        .Select(p => new { p.Id, p.Codigo, p.Nombre, p.Costo, p.Precio })
        .ToListAsync();
    return Results.Ok(new
    {
        g.IdGrupoProducto,
        g.NombreGrupo,
        g.Costo,
        g.PrecioGrupo,
        g.Ganancia,
        g.GananciaIndividual,
        miembros
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/grupos", async (GrupoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarGrupos(sesion.Rol ?? "")) return Results.Forbid();
    if (string.IsNullOrWhiteSpace(pedido.NombreGrupo))
        return Results.BadRequest("El nombre del grupo es obligatorio.");
    var g = new GrupoProductos
    {
        IdLocal = sesion.IdLocal.Value,
        NombreGrupo = pedido.NombreGrupo.Trim(),
        Costo = pedido.Costo,
        PrecioGrupo = pedido.PrecioGrupo,
        Ganancia = pedido.Ganancia,
        GananciaIndividual = pedido.GananciaIndividual
    };
    db.GrupoProductos.Add(g);
    await db.SaveChangesAsync();
    return Results.Created($"/grupos/{g.IdGrupoProducto}", new
    {
        g.IdGrupoProducto,
        g.NombreGrupo,
        g.Costo,
        g.PrecioGrupo,
        cantidad = 0
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/grupos/{id:int}", async (int id, GrupoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarGrupos(sesion.Rol ?? "")) return Results.Forbid();
    if (string.IsNullOrWhiteSpace(pedido.NombreGrupo))
        return Results.BadRequest("El nombre del grupo es obligatorio.");
    var g = await db.GrupoProductos.FirstOrDefaultAsync(x => x.IdGrupoProducto == id);
    if (g == null) return Results.NotFound();
    g.NombreGrupo = pedido.NombreGrupo.Trim();
    g.Costo = pedido.Costo;
    g.PrecioGrupo = pedido.PrecioGrupo;
    g.Ganancia = pedido.Ganancia;
    g.GananciaIndividual = pedido.GananciaIndividual;
    var miembros = await db.Productos.Where(p => p.IdGrupoProducto == id).ToListAsync();
    ServicioGrupo.AplicarAMiembros(g, miembros, sesion.NombreUsuario ?? "sistema");
    await db.SaveChangesAsync();
    return Results.Ok(new
    {
        g.IdGrupoProducto,
        g.NombreGrupo,
        g.Costo,
        g.PrecioGrupo,
        cantidad = miembros.Count
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapDelete("/grupos/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarGrupos(sesion.Rol ?? "")) return Results.Forbid();
    var g = await db.GrupoProductos.FirstOrDefaultAsync(x => x.IdGrupoProducto == id);
    if (g == null) return Results.NotFound();
    var miembros = await db.Productos.Where(p => p.IdGrupoProducto == id).ToListAsync();
    ServicioGrupo.Eliminar(miembros, sesion.NombreUsuario ?? "sistema");
    db.GrupoProductos.Remove(g);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/grupos/{id:int}/miembros", async (int id, MiembrosGrupoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarGrupos(sesion.Rol ?? "")) return Results.Forbid();
    var g = await db.GrupoProductos.FirstOrDefaultAsync(x => x.IdGrupoProducto == id);
    if (g == null) return Results.NotFound();
    var ids = pedido.IdsProducto ?? [];
    var candidatos = await db.Productos
        .Where(p => p.IdGrupoProducto == id || ids.Contains(p.Id))
        .ToListAsync();
    try
    {
        ServicioGrupo.ReemplazarMiembros(g, candidatos, ids, sesion.NombreUsuario ?? "sistema");
        await db.SaveChangesAsync();
        return Results.Ok(new { g.IdGrupoProducto, cantidad = ids.Count });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/configuracion/local", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == sesion.IdLocal);
    if (local == null) return Results.NotFound();
    var configuracion = await LeerConfiguracionLocal(db, local);
    await db.SaveChangesAsync();
    configuracion.FormatoTicket = ServicioTicket.FormatoODefault(configuracion.FormatoTicket);
    return Results.Ok(configuracion);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/configuracion/ticket", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == sesion.IdLocal);
    if (local == null) return Results.NotFound();
    var configuracion = await LeerConfiguracionLocal(db, local);
    await db.SaveChangesAsync();
    return Results.Ok(new
    {
        formatoTicket = ServicioTicket.FormatoODefault(configuracion.FormatoTicket),
        imprimirTicketAlCobrar = configuracion.ImprimirTicketAlCobrar,
        nombreLocal = local.Nombre
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/carteles", async (CartelesPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeImprimirCarteles(sesion.Rol ?? "")) return Results.Forbid();
    var ids = pedido.IdsProducto.Distinct().ToList();
    if (ids.Count == 0) return Results.BadRequest("Elegí al menos un producto.");
    if (ids.Count > 300) return Results.BadRequest("Máximo 300 carteles por vez.");
    try
    {
        var tamano = ServicioCarteles.NormalizarTamano(pedido.Tamano);
        var productos = await db.Productos.Where(p => ids.Contains(p.Id)).OrderBy(p => p.Nombre).ToListAsync();
        var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == sesion.IdLocal);
        var plantilla = await db.PlantillasCartel.FirstOrDefaultAsync(p => p.Tamano == tamano);
        return Results.Ok(new
        {
            tamano,
            nombreLocal = local?.Nombre ?? "",
            plantilla = DataUrl(plantilla),
            hojas = ServicioCarteles.ArmarHojas(productos, tamano)
        });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/carteles/plantillas", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeImprimirCarteles(sesion.Rol ?? "")) return Results.Forbid();
    var plantillas = await db.PlantillasCartel.ToListAsync();
    return Results.Ok(new
    {
        grande = DataUrl(plantillas.FirstOrDefault(p => p.Tamano == ServicioCarteles.Grande)),
        chico = DataUrl(plantillas.FirstOrDefault(p => p.Tamano == ServicioCarteles.Chico))
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/carteles/plantillas/{tamano}", async (string tamano, PlantillaCartelPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    try
    {
        var t = ServicioCarteles.NormalizarTamano(tamano);
        var datos = DecodificarBase64(pedido.Imagen);
        var tipo = ServicioCarteles.ValidarPlantilla(datos);
        var fila = await db.PlantillasCartel.FirstOrDefaultAsync(p => p.Tamano == t);
        if (fila == null)
        {
            fila = new PlantillaCartel { IdLocal = sesion.IdLocal.Value, Tamano = t };
            db.PlantillasCartel.Add(fila);
        }
        fila.TipoContenido = tipo;
        fila.Datos = datos;
        await db.SaveChangesAsync();
        return Results.Ok(new { tamano = t, plantilla = DataUrl(fila) });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapDelete("/carteles/plantillas/{tamano}", async (string tamano, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    try
    {
        var t = ServicioCarteles.NormalizarTamano(tamano);
        var fila = await db.PlantillasCartel.FirstOrDefaultAsync(p => p.Tamano == t);
        if (fila != null)
        {
            db.PlantillasCartel.Remove(fila);
            await db.SaveChangesAsync();
        }
        return Results.NoContent();
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/configuracion/local", async (ConfiguracionLocalPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == sesion.IdLocal);
    if (local == null) return Results.NotFound();
    string formato;
    try
    {
        formato = ServicioTicket.NormalizarFormato(pedido.FormatoTicket);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    var anterior = await LeerConfiguracionLocal(db, local);
    var cambioPrecios = anterior.FactorGanancia != pedido.FactorGanancia || anterior.Iva != pedido.Iva;
    local.Nombre = pedido.NombreLocal.Trim();
    if (local.Nombre.Length == 0) return Results.BadRequest("El nombre del local es obligatorio.");
    GuardarConfiguracion(db, local.IdLocal, "FactorGanancia", pedido.FactorGanancia);
    GuardarConfiguracion(db, local.IdLocal, "FactorGananciaSector", pedido.FactorGananciaSector);
    GuardarConfiguracion(db, local.IdLocal, "FactorGananciaGenerico", pedido.FactorGananciaGenerico);
    GuardarConfiguracion(db, local.IdLocal, "IVA", pedido.Iva);
    GuardarConfiguracion(db, local.IdLocal, "StockRigido", pedido.StockRigido);
    GuardarConfiguracion(db, local.IdLocal, "UmbralStockBajo", pedido.UmbralStockBajo);
    GuardarConfiguracion(db, local.IdLocal, "EmpleadoPuedeModificarPrecios", pedido.EmpleadoPuedeModificarPrecios);
    GuardarConfiguracion(db, local.IdLocal, "FormatoTicket", formato);
    GuardarConfiguracion(db, local.IdLocal, "ImprimirTicketAlCobrar", pedido.ImprimirTicketAlCobrar);
    GuardarConfiguracion(db, local.IdLocal, "CantidadCajas", pedido.CantidadCajas);
    var recalculados = 0;
    if (pedido.RecalcularPrecios && cambioPrecios)
    {
        var productos = await db.Productos
            .Where(p => !p.ProductoSector && p.IdGrupoProducto == 0 && !p.GananciaIndividual)
            .ToListAsync();
        foreach (var producto in productos)
        {
            producto.Precio = PrecioVenta.CalcularLista(producto.Costo, pedido.FactorGanancia, pedido.Iva);
            producto.FechaModificacion = DateTimeOffset.UtcNow;
            producto.UsuarioModificacion = sesion.NombreUsuario ?? "sistema";
            recalculados++;
        }
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { recalculados });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/usuarios", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarUsuarios(sesion.Rol ?? "")) return Results.Forbid();
    var localesCuenta = await LocalesDeLaCuenta(db, sesion);
    var locales = await db.Locales.AsNoTracking()
        .Where(l => localesCuenta.Contains(l.IdLocal) && l.Activo)
        .OrderBy(l => l.Nombre)
        .ToListAsync();
    var idsLocal = locales.Select(l => l.IdLocal).ToList();
    var asignaciones = await db.UsuarioLocales.AsNoTracking().Where(x => idsLocal.Contains(x.IdLocal)).ToListAsync();
    var idsUsuarios = asignaciones.Select(a => a.IdUsuario).Distinct().ToList();
    var usuarios = await db.Usuarios.AsNoTracking()
        .Where(u => idsUsuarios.Contains(u.Id) && (u.Rol == Roles.Empleado || u.Rol == Roles.Socio))
        .OrderBy(u => u.Nombre).ThenBy(u => u.NombreUsuario)
        .ToListAsync();
    var nombres = locales.ToDictionary(l => l.IdLocal, l => l.Nombre);
    return Results.Ok(new
    {
        locales = locales.Select(l => new { l.IdLocal, l.Nombre }),
        usuarios = usuarios.Select(u => new
        {
            u.Id,
            nombre = string.IsNullOrWhiteSpace(u.Nombre) ? u.NombreUsuario : u.Nombre,
            u.NombreUsuario,
            u.Rol,
            u.Activo,
            locales = asignaciones.Where(a => a.IdUsuario == u.Id && nombres.ContainsKey(a.IdLocal))
                .Select(a => new { a.IdLocal, nombre = nombres[a.IdLocal] })
        })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/usuarios", async (UsuarioPedido pedido, AppDbContext db, SesionActual sesion, PasswordHasher<Usuario> hasher) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!PuedeCrearUsuario(sesion.Rol, pedido.Rol)) return Results.Forbid();
    var validacion = ValidarUsuarioPedido(pedido, true);
    if (validacion != null) return Results.BadRequest(validacion);
    if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == pedido.NombreUsuario.Trim()))
        return Results.Conflict("Ya existe ese nombre de usuario.");
    var locales = await LocalesDeLaCuenta(db, sesion);
    var idsLocal = pedido.IdsLocal.Distinct().ToList();
    if (!idsLocal.All(locales.Contains)) return Results.Forbid();
    var usuario = new Usuario
    {
        Nombre = pedido.Nombre.Trim(),
        NombreUsuario = pedido.NombreUsuario.Trim(),
        Rol = pedido.Rol,
        Activo = true
    };
    usuario.HashClave = hasher.HashPassword(usuario, pedido.Clave!);
    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();
    db.UsuarioLocales.AddRange(idsLocal.Select(idLocal => new UsuarioLocal { IdUsuario = usuario.Id, IdLocal = idLocal }));
    await db.SaveChangesAsync();
    return Results.Created($"/usuarios/{usuario.Id}", new { usuario.Id, usuario.Nombre, usuario.NombreUsuario, usuario.Rol, usuario.Activo });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/usuarios/{id:int}", async (int id, UsuarioPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    if (usuario == null) return Results.NotFound();
    var locales = await LocalesDeLaCuenta(db, sesion);
    if (!await EstaEnLaCuenta(db, id, locales)) return Results.NotFound();
    if (!PuedeEditarUsuario(sesion.Rol, usuario.Rol, pedido.Rol)) return Results.Forbid();
    var validacion = ValidarUsuarioPedido(pedido, false);
    if (validacion != null) return Results.BadRequest(validacion);
    if (await db.Usuarios.AnyAsync(u => u.Id != id && u.NombreUsuario == pedido.NombreUsuario.Trim()))
        return Results.Conflict("Ya existe ese nombre de usuario.");
    var idsLocal = pedido.IdsLocal.Distinct().ToList();
    if (!idsLocal.All(locales.Contains)) return Results.Forbid();
    usuario.Nombre = pedido.Nombre.Trim();
    usuario.NombreUsuario = pedido.NombreUsuario.Trim();
    usuario.Rol = pedido.Rol;
    var actuales = await db.UsuarioLocales.Where(x => x.IdUsuario == id && locales.Contains(x.IdLocal)).ToListAsync();
    db.UsuarioLocales.RemoveRange(actuales);
    db.UsuarioLocales.AddRange(idsLocal.Select(idLocal => new UsuarioLocal { IdUsuario = id, IdLocal = idLocal }));
    await db.SaveChangesAsync();
    return Results.Ok(new { usuario.Id, usuario.Nombre, usuario.NombreUsuario, usuario.Rol, usuario.Activo });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/usuarios/{id:int}/activar", async (int id, AppDbContext db, SesionActual sesion) =>
    await CambiarEstadoUsuario(id, true, db, sesion))
    .RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/usuarios/{id:int}/desactivar", async (int id, AppDbContext db, SesionActual sesion) =>
    await CambiarEstadoUsuario(id, false, db, sesion))
    .RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/usuarios/{id:int}/resetear-clave", async (int id, ResetearClavePedido pedido, AppDbContext db, SesionActual sesion, PasswordHasher<Usuario> hasher) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    var locales = await LocalesDeLaCuenta(db, sesion);
    if (usuario == null || !await EstaEnLaCuenta(db, id, locales)) return Results.NotFound();
    if (!PuedeEditarUsuario(sesion.Rol, usuario.Rol, usuario.Rol)) return Results.Forbid();
    if (string.IsNullOrWhiteSpace(pedido.Clave) || pedido.Clave.Length < 4) return Results.BadRequest("La clave debe tener al menos 4 caracteres.");
    usuario.HashClave = hasher.HashPassword(usuario, pedido.Clave);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/admin/usuarios", async (AppDbContext db, SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var cuentas = await db.Usuarios.AsNoTracking()
        .Where(u => u.Rol == Roles.Dueno || u.Rol == Roles.Admin)
        .OrderBy(u => u.Nombre).ThenBy(u => u.NombreUsuario)
        .Take(maxPagina)
        .ToListAsync();
    var ids = cuentas.Select(u => u.Id).ToList();
    var vinculos = await db.UsuarioLocales.AsNoTracking().Where(x => ids.Contains(x.IdUsuario)).ToListAsync();
    var idLocales = vinculos.Select(x => x.IdLocal).Distinct().ToList();
    var locales = await db.Locales.AsNoTracking().Where(l => idLocales.Contains(l.IdLocal)).ToDictionaryAsync(l => l.IdLocal);
    var ahora = DateTimeOffset.UtcNow;
    return Results.Ok(cuentas.Select(u => new
    {
        u.Id,
        nombre = string.IsNullOrWhiteSpace(u.Nombre) ? u.NombreUsuario : u.Nombre,
        u.NombreUsuario,
        u.Email,
        u.Rol,
        u.Activo,
        locales = vinculos.Where(v => v.IdUsuario == u.Id && locales.ContainsKey(v.IdLocal)).Select(v =>
        {
            var local = locales[v.IdLocal];
            var abono = ServicioAbono.Estado(local, ahora);
            return new { local.IdLocal, local.Nombre, abono.Estado, abono.Vence };
        })
    }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/admin/usuarios/{id:int}/creados", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var dueno = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    if (dueno == null || !ServicioAdmin.EsCuentaRegistrada(dueno.Rol)) return Results.NotFound();
    var idsLocal = await db.UsuarioLocales.Where(x => x.IdUsuario == id).Select(x => x.IdLocal).ToListAsync();
    var idsUsuarios = await db.UsuarioLocales
        .Where(x => idsLocal.Contains(x.IdLocal) && x.IdUsuario != id)
        .Select(x => x.IdUsuario).Distinct().ToListAsync();
    var creados = await db.Usuarios.AsNoTracking()
        .Where(u => idsUsuarios.Contains(u.Id) && (u.Rol == Roles.Empleado || u.Rol == Roles.Socio))
        .OrderBy(u => u.Nombre).ThenBy(u => u.NombreUsuario)
        .Take(maxPagina)
        .ToListAsync();
    return Results.Ok(creados.Select(u => new
    {
        u.Id,
        nombre = string.IsNullOrWhiteSpace(u.Nombre) ? u.NombreUsuario : u.Nombre,
        u.NombreUsuario,
        u.Rol,
        u.Activo
    }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/usuarios/{id:int}/locales", async (int id, AltaLocalAdminPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    if (usuario == null) return Results.NotFound();
    try
    {
        ServicioAdmin.ValidarDueno(usuario.Rol);
        var yaTieneLocal = await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == usuario.Id);
        var local = ServicioAdmin.AltaLocal(pedido.Nombre, DateTimeOffset.UtcNow, yaTieneLocal);
        await using var transaccion = await db.Database.BeginTransactionAsync();
        db.Locales.Add(local);
        await db.SaveChangesAsync();
        db.UsuarioLocales.Add(new UsuarioLocal { IdUsuario = usuario.Id, IdLocal = local.IdLocal });
        db.MetodosPago.AddRange(ServicioRegistro.MediosIniciales.Select(m => new MetodoPago { IdLocal = local.IdLocal, Descripcion = m }));
        var copiados = await CopiarCatalogoAlLocal(db, usuario.Id, local.IdLocal, pedido, sesion.NombreUsuario ?? "admin");
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return Results.Created($"/admin/usuarios/{id}/locales/{local.IdLocal}", new { local.IdLocal, local.Nombre, estado = local.EstadoAbono, local.Vence, cantidadProductos = copiados });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/locales/{idLocal:int}/pago", async (int idLocal, PagoAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == idLocal);
    if (local == null) return Results.NotFound();
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        var abono = ServicioAbono.RegistrarPagoAdmin(local, DateTimeOffset.UtcNow, pedido.Canal);
        db.Abonos.Add(abono);
        await db.SaveChangesAsync();
        return Results.Ok(new { local.IdLocal, estado = local.EstadoAbono, local.Vence, abono.Canal });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/locales/{idLocal:int}/anular-pago", async (int idLocal, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == idLocal);
    if (local == null) return Results.NotFound();
    var abono = await db.Abonos.IgnoreQueryFilters()
        .Where(a => a.IdLocal == idLocal && a.Estado == Abono.Aprobado)
        .OrderByDescending(a => a.FechaAcreditacion)
        .ThenByDescending(a => a.Id)
        .FirstOrDefaultAsync();
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        ServicioAbono.AnularPagoAdmin(local, abono);
        await db.SaveChangesAsync();
        return Results.Ok(new { local.IdLocal, estado = local.EstadoAbono, local.Vence });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/admin/locales", async (AppDbContext db, SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var ahora = DateTimeOffset.UtcNow;
    var locales = await db.Locales.AsNoTracking().OrderBy(l => l.Nombre).ToListAsync();
    var ids = locales.Select(l => l.IdLocal).ToList();
    var vinculos = await db.UsuarioLocales.AsNoTracking().Where(v => ids.Contains(v.IdLocal)).ToListAsync();
    var idsUsuarios = vinculos.Select(v => v.IdUsuario).Distinct().ToList();
    var usuarios = await db.Usuarios.AsNoTracking()
        .Where(u => idsUsuarios.Contains(u.Id))
        .Select(u => new { u.Id, u.Nombre, u.NombreUsuario, u.Rol })
        .ToListAsync();
    var pares = vinculos.Select(v =>
    {
        var rol = usuarios.FirstOrDefault(u => u.Id == v.IdUsuario)?.Rol ?? "";
        return (v.IdLocal, Rol: rol);
    }).ToList();
    var estados = locales.Where(l => l.Activo).Select(l => ServicioAbono.Estado(l, ahora).Estado).ToList();
    var telefono = await TelefonoVentas(db);
    return Results.Ok(new
    {
        totalMes = ServicioAbono.TotalDelMes(estados),
        precioMensual = ServicioAbono.PrecioMensual,
        localesPagos = estados.Count(ServicioAbono.CuentaEnElMes),
        telefono = ServicioAdmin.MostrarTelefono(telefono),
        email = await EmailVentas(db),
        items = locales.Take(maxPagina).Select(l =>
        {
            var abono = ServicioAbono.Estado(l, ahora);
            var idDueno = vinculos
                .Where(v => v.IdLocal == l.IdLocal)
                .Select(v => usuarios.FirstOrDefault(u => u.Id == v.IdUsuario && u.Rol == Roles.Dueno))
                .FirstOrDefault(u => u != null);
            var dueno = idDueno;
            var cliente = dueno == null ? "" : string.IsNullOrWhiteSpace(dueno.Nombre) ? dueno.NombreUsuario : dueno.Nombre;
            return new
            {
                l.IdLocal,
                l.Nombre,
                l.Activo,
                cliente,
                abono.Estado,
                abono.Vence,
                empleados = ServicioAdmin.Empleados(pares, l.IdLocal)
            };
        })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/locales", async (AltaLocalAdminPedido pedido, AppDbContext db, SesionActual sesion, PasswordHasher<Usuario> hasher) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    try
    {
        Usuario? nuevo = null;
        Usuario dueno;
        var yaTieneLocal = false;
        if (pedido.IdDueno > 0)
        {
            dueno = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == pedido.IdDueno) ?? throw new ErrorNegocio("Elegí un dueño.");
            ServicioAdmin.ValidarDueno(dueno.Rol);
            yaTieneLocal = await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == dueno.Id);
        }
        else
        {
            nuevo = ServicioAdmin.NuevoDueno(pedido.NombreDueno, pedido.Email, pedido.NombreUsuario, pedido.ClaveDueno);
            if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == nuevo.NombreUsuario || (nuevo.Email != "" && u.Email == nuevo.Email)))
                return Results.Conflict("Ya existe una cuenta con ese usuario o mail.");
            nuevo.HashClave = hasher.HashPassword(nuevo, pedido.ClaveDueno);
            dueno = nuevo;
        }

        var local = ServicioAdmin.AltaLocal(pedido.Nombre, DateTimeOffset.UtcNow, yaTieneLocal);
        await using var transaccion = await db.Database.BeginTransactionAsync();
        if (nuevo != null) db.Usuarios.Add(nuevo);
        db.Locales.Add(local);
        await db.SaveChangesAsync();
        db.UsuarioLocales.Add(new UsuarioLocal { IdUsuario = dueno.Id, IdLocal = local.IdLocal });
        db.MetodosPago.AddRange(ServicioRegistro.MediosIniciales.Select(m => new MetodoPago { IdLocal = local.IdLocal, Descripcion = m }));
        var copiados = await CopiarCatalogoAlLocal(db, dueno.Id, local.IdLocal, pedido, sesion.NombreUsuario ?? "admin");
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return Results.Created($"/admin/locales/{local.IdLocal}", new { local.IdLocal, local.Nombre, dueno.Id, cantidadProductos = copiados });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/locales/{idLocal:int}/vencimiento", async (int idLocal, VencimientoAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == idLocal);
    if (local == null) return Results.NotFound();
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        ServicioAdmin.CambiarVencimiento(local, ServicioAdmin.InterpretarVencimiento(pedido.Vence));
        await db.SaveChangesAsync();
        return Results.Ok(new { local.IdLocal, local.Vence });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/locales/{idLocal:int}/desactivar", async (int idLocal, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
    await CambiarActivoLocal(idLocal, false, pedido, db, sesion, config)).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/locales/{idLocal:int}/activar", async (int idLocal, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
    await CambiarActivoLocal(idLocal, true, pedido, db, sesion, config)).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/admin/ventas/telefono", async (AppDbContext db, SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var telefono = await TelefonoVentas(db);
    return Results.Ok(new { telefono = ServicioAdmin.MostrarTelefono(telefono) });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/admin/ventas/telefono", async (TelefonoVentasPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        var valor = ServicioAdmin.GuardarTelefonoVentas(pedido.Telefono);
        var email = ServicioAdmin.GuardarEmailVentas(pedido.Email);
        await GuardarAjuste(db, Ajuste.TelefonoVentas, valor);
        await GuardarAjuste(db, Ajuste.EmailVentas, email);
        await db.SaveChangesAsync();
        return Results.Ok(new { telefono = ServicioAdmin.MostrarTelefono(valor), email });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ventas/contacto", async (AppDbContext db, IDataProtectionProvider proteccion) =>
{
    var telefono = await TelefonoVentas(db);
    return Results.Ok(new
    {
        telefono = ServicioAdmin.MostrarTelefono(telefono),
        url = UrlContacto(telefono),
        email = await EmailVentas(db),
        formularioActivo = await FormularioContactoActivo(db),
        apertura = AperturaContacto(proteccion)
    });
}).RequireRateLimiting("api");

app.MapPost("/ventas/contacto", async (MensajeContactoPedido pedido, AppDbContext db, IDataProtectionProvider proteccion) =>
{
    if (ServicioContacto.EsCebo(pedido.Sitio)) return Results.Ok();
    var transcurrido = EdadApertura(proteccion, pedido.Apertura);
    if (transcurrido == null)
        return Results.BadRequest("Volvé a cargar la página y enviá de nuevo.");
    var rechazo = ServicioContacto.RechazoPorTiempo(transcurrido.Value);
    if (rechazo != null)
        return Results.BadRequest(rechazo);
    if (!await FormularioContactoActivo(db))
        return Results.BadRequest("El formulario de contacto está desactivado.");
    try
    {
        var mensaje = ServicioContacto.Nuevo(pedido.Nombre, pedido.Correo, pedido.Mensaje, DateTimeOffset.UtcNow);
        db.MensajesContacto.Add(mensaje);
        await db.SaveChangesAsync();
        return Results.Created($"/ventas/contacto/{mensaje.Id}", new { enviado = true });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireRateLimiting("contacto");

app.MapGet("/admin/contacto", async (AppDbContext db, SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    await PurgarMensajesLeidos(db);
    var mensajes = await db.MensajesContacto.AsNoTracking()
        .OrderBy(m => m.Leido != null)
        .ThenByDescending(m => m.Creado)
        .Take(maxPagina)
        .ToListAsync();
    return Results.Ok(new
    {
        activo = await FormularioContactoActivo(db),
        items = mensajes.Select(m => new
        {
            m.Id,
            m.Nombre,
            m.Correo,
            m.Mensaje,
            m.Creado,
            m.Leido,
            vence = m.Leido?.AddDays(ServicioContacto.DiasHastaBorrar)
        })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/contacto/{id:int}/leido", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var mensaje = await db.MensajesContacto.FirstOrDefaultAsync(m => m.Id == id);
    if (mensaje == null) return Results.NotFound();
    ServicioContacto.MarcarLeido(mensaje, DateTimeOffset.UtcNow);
    await db.SaveChangesAsync();
    return Results.Ok(new { mensaje.Id, mensaje.Leido, vence = mensaje.Leido?.AddDays(ServicioContacto.DiasHastaBorrar) });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/admin/contacto", async (FormularioContactoPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        await GuardarAjuste(db, Ajuste.FormularioContacto, pedido.Activo ? "1" : "0");
        await db.SaveChangesAsync();
        return Results.Ok(new { activo = pedido.Activo });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/usuarios/{id:int}/admin", async (int id, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    if (usuario == null) return Results.NotFound();
    if (usuario.Rol == Roles.Admin) return Results.BadRequest("Ese usuario ya es administrador.");
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    usuario.Rol = Roles.Admin;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/usuarios/{id:int}/clave", async (int id, CambioClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, PasswordHasher<Usuario> hasher, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    if (usuario == null) return Results.NotFound();
    try
    {
        var clave = ServicioAdmin.ValidarCambioClave(usuario.Rol, pedido.Clave, pedido.ClaveSecreta, config["Admin:Clave"]);
        usuario.HashClave = hasher.HashPassword(usuario, clave);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/admin/usuarios/{id:int}/bloquear", async (int id, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    if (usuario == null) return Results.NotFound();
    var otros = await db.Usuarios.CountAsync(u => u.Rol == Roles.Admin && u.Activo && u.Id != id);
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        ServicioAdmin.AsegurarAdminRestante(usuario.Rol, otros);
        ServicioAdmin.Bloquear(usuario, sesion.IdUsuario.Value);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapDelete("/admin/usuarios/{id:int}", async (int id, [FromBody] ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config) =>
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    if (usuario == null) return Results.NotFound();
    var otros = await db.Usuarios.CountAsync(u => u.Rol == Roles.Admin && u.Activo && u.Id != id);
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        ServicioAdmin.ValidarEliminar(sesion.IdUsuario.Value, id);
        ServicioAdmin.AsegurarAdminRestante(usuario.Rol, otros);
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    var vinculos = await db.UsuarioLocales.Where(x => x.IdUsuario == id).ToListAsync();
    db.UsuarioLocales.RemoveRange(vinculos);
    db.Usuarios.Remove(usuario);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/productos/grupos", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var grupos = await db.GrupoProductos.OrderBy(g => g.NombreGrupo)
        .Select(g => new { g.IdGrupoProducto, g.NombreGrupo })
        .ToListAsync();
    return Results.Ok(grupos);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/medios-pago", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var medios = await db.MetodosPago.OrderBy(m => m.Descripcion).ToListAsync();
    return Results.Ok(medios.Select(m => new { m.Id, m.Descripcion, m.Activo, m.Integracion }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/medios-pago", async (MetodoPagoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarMetodosPago(sesion.Rol ?? "")) return Results.Forbid();
    var descripcion = pedido.Descripcion.Trim();
    if (descripcion.Length == 0) return Results.BadRequest("La descripción es obligatoria.");
    if (await db.MetodosPago.AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower()))
        return Results.Conflict("Ya existe ese medio de pago.");
    string integracion;
    try { integracion = IntegracionPago.Normalizar(pedido.Integracion); }
    catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
    var medio = new MetodoPago { IdLocal = sesion.IdLocal.Value, Descripcion = descripcion, Integracion = integracion };
    db.MetodosPago.Add(medio);
    await db.SaveChangesAsync();
    return Results.Created($"/medios-pago/{medio.Id}", new { medio.Id, medio.Descripcion, medio.Activo, medio.Integracion });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/medios-pago/{id:int}", async (int id, MetodoPagoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarMetodosPago(sesion.Rol ?? "")) return Results.Forbid();
    var medio = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == id);
    if (medio == null) return Results.NotFound();
    var descripcion = pedido.Descripcion.Trim();
    if (descripcion.Length == 0) return Results.BadRequest("La descripción es obligatoria.");
    if (await db.MetodosPago.AnyAsync(m => m.Id != id && m.Descripcion.ToLower() == descripcion.ToLower()))
        return Results.Conflict("Ya existe ese medio de pago.");
    string integracion;
    try { integracion = IntegracionPago.Normalizar(pedido.Integracion); }
    catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
    medio.Descripcion = descripcion;
    medio.Integracion = integracion;
    await db.SaveChangesAsync();
    return Results.Ok(new { medio.Id, medio.Descripcion, medio.Activo, medio.Integracion });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapDelete("/medios-pago/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarMetodosPago(sesion.Rol ?? "")) return Results.Forbid();
    var medio = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == id);
    if (medio == null) return Results.NotFound();
    medio.Activo = false;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/medios-pago/{id:int}/activar", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarMetodosPago(sesion.Rol ?? "")) return Results.Forbid();
    var medio = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == id);
    if (medio == null) return Results.NotFound();
    medio.Activo = true;
    await db.SaveChangesAsync();
    return Results.Ok(new { medio.Id, medio.Descripcion, medio.Activo, medio.Integracion });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/proveedores", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var proveedores = await db.Proveedores.OrderBy(p => p.Nombre).ToListAsync();
    var productos = await db.Productos.Where(p => p.IdProveedor > 0)
        .GroupBy(p => p.IdProveedor).Select(g => new { g.Key, Cantidad = g.Count() })
        .ToDictionaryAsync(x => x.Key, x => x.Cantidad);
    var ultimas = await db.OrdenesCompra
        .GroupBy(o => o.IdProveedor).Select(g => new { g.Key, Fecha = g.Max(o => o.Fecha) })
        .ToDictionaryAsync(x => x.Key, x => x.Fecha);
    return Results.Ok(proveedores.Select(p => new
    {
        p.Id,
        p.Nombre,
        p.TipoProductos,
        p.Telefono,
        p.Activo,
        cantidadProductos = productos.GetValueOrDefault(p.Id),
        ultimaOrden = ultimas.TryGetValue(p.Id, out var f) ? f : (DateTimeOffset?)null
    }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/proveedores/plantilla", (SesionActual sesion) =>
{
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    return Results.File(ExcelProveedor.Plantilla(), ExcelProveedor.TipoContenido, "plantilla-proveedor.xlsx");
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/proveedores/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var p = await db.Proveedores.FirstOrDefaultAsync(x => x.Id == id);
    if (p == null) return Results.NotFound();
    var ultima = await db.OrdenesCompra.Include(o => o.Lineas)
        .Where(o => o.IdProveedor == id).OrderByDescending(o => o.Fecha).FirstOrDefaultAsync();
    return Results.Ok(new
    {
        p.Id,
        p.Nombre,
        p.TipoProductos,
        p.Telefono,
        p.Activo,
        urlContacto = UrlContacto(p.Telefono),
        cantidadProductos = await db.Productos.CountAsync(x => x.IdProveedor == id),
        ultimaOrden = ultima == null ? null : OrdenDto(ultima)
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/proveedores", async (ProveedorPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var error = ValidarProveedorPedido(pedido);
    if (error != null) return Results.BadRequest(error);
    var nombre = pedido.Nombre.Trim();
    if (await db.Proveedores.AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower()))
        return Results.Conflict("Ya existe ese proveedor.");
    var proveedor = new Proveedor
    {
        IdLocal = sesion.IdLocal.Value,
        Nombre = nombre,
        TipoProductos = pedido.TipoProductos.Trim(),
        Telefono = pedido.Telefono.Trim()
    };
    db.Proveedores.Add(proveedor);
    await db.SaveChangesAsync();
    return Results.Created($"/proveedores/{proveedor.Id}", proveedor);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/proveedores/{id:int}", async (int id, ProveedorPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == id);
    if (proveedor == null) return Results.NotFound();
    var error = ValidarProveedorPedido(pedido);
    if (error != null) return Results.BadRequest(error);
    var nombre = pedido.Nombre.Trim();
    if (await db.Proveedores.AnyAsync(p => p.Id != id && p.Nombre.ToLower() == nombre.ToLower()))
        return Results.Conflict("Ya existe ese proveedor.");
    proveedor.Nombre = nombre;
    proveedor.TipoProductos = pedido.TipoProductos.Trim();
    proveedor.Telefono = pedido.Telefono.Trim();
    await db.SaveChangesAsync();
    return Results.Ok(proveedor);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapDelete("/proveedores/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == id);
    if (proveedor == null) return Results.NotFound();
    if (await db.OrdenesCompra.AnyAsync(o => o.IdProveedor == id))
    {
        proveedor.Activo = false;
        await db.SaveChangesAsync();
        return Results.Ok(new { desactivado = true });
    }
    await db.Productos.Where(p => p.IdProveedor == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.IdProveedor, 0));
    db.Proveedores.Remove(proveedor);
    await db.SaveChangesAsync();
    return Results.Ok(new { desactivado = false });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/proveedores/{id:int}/activar", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == id);
    if (proveedor == null) return Results.NotFound();
    proveedor.Activo = true;
    await db.SaveChangesAsync();
    return Results.Ok(proveedor);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/proveedores/{id:int}/catalogo/revisar", async (int id, ArchivoExcelPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    if (!await db.Proveedores.AnyAsync(p => p.Id == id)) return Results.NotFound();
    try
    {
        var filas = ExcelProveedor.LeerCatalogo(DecodificarBase64(pedido.Archivo));
        var config = await ConfiguracionDelLocal(db, sesion);
        var productos = await ProductosPorCodigo(db, filas);
        var revision = ServicioCatalogoProveedor.Revisar(filas, productos, id, config.FactorGanancia, config.Iva);
        return Results.Ok(new { filas, revision.Validas, revision.Invalidas });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/proveedores/{id:int}/catalogo/confirmar", async (int id, ConfirmarCatalogoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    if (pedido.Filas.Count == 0) return Results.BadRequest("No hay productos para importar.");
    if (pedido.Filas.Count > ExcelProveedor.MaxFilas) return Results.BadRequest($"Máximo {ExcelProveedor.MaxFilas} productos por vez.");
    if (!await db.Proveedores.AnyAsync(p => p.Id == id && p.Activo)) return Results.NotFound();
    var filas = pedido.Filas.Select(f => new FilaCatalogo
    {
        Fila = f.Fila,
        Codigo = f.Codigo ?? "",
        Nombre = f.Nombre ?? "",
        Costo = f.Costo,
        Cantidad = f.Cantidad
    }).ToList();
    var config = await ConfiguracionDelLocal(db, sesion);
    var productos = await ProductosPorCodigo(db, filas);
    var resultado = ServicioCatalogoProveedor.Aplicar(
        filas, productos, id, sesion.IdLocal.Value, config.FactorGanancia, config.Iva, sesion.NombreUsuario ?? "sistema");
    db.Productos.AddRange(resultado.Nuevos);
    await db.SaveChangesAsync();
    return Results.Ok(new { creados = resultado.Nuevos.Count, resultado.Actualizados, resultado.Invalidas });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/proveedores/{id:int}/ordenes", async (int id, OrdenCompraPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    if (pedido.Lineas.Count > 500) return Results.BadRequest("Máximo 500 productos por orden.");
    var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == id && p.Activo);
    if (proveedor == null) return Results.NotFound();
    try
    {
        var ids = pedido.Lineas.Select(l => l.IdProducto).Distinct().ToList();
        var productos = await db.Productos.Where(p => ids.Contains(p.Id)).ToListAsync();
        var lineas = pedido.Lineas.Select(l => new LineaOrdenPedida { IdProducto = l.IdProducto, Cantidad = l.Cantidad });
        var orden = ServicioOrdenCompra.Armar(
            id, sesion.IdLocal.Value, lineas, productos, sesion.IdUsuario, sesion.NombreUsuario ?? "sistema", DateTimeOffset.UtcNow);
        db.OrdenesCompra.Add(orden);
        await db.SaveChangesAsync();
        var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
        var texto = ServicioOrdenCompra.TextoWhatsApp(local.Nombre, proveedor.Nombre, orden.Fecha, orden.Lineas);
        return Results.Ok(new
        {
            orden = OrdenDto(orden),
            nombreLocal = local.Nombre,
            textoWhatsApp = texto,
            urlWhatsApp = UrlContacto(proveedor.Telefono, texto)
        });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ordenes-compra/{id:int}/excel", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var orden = await db.OrdenesCompra.Include(o => o.Lineas).FirstOrDefaultAsync(o => o.Id == id);
    if (orden == null) return Results.NotFound();
    var proveedor = await db.Proveedores.FirstAsync(p => p.Id == orden.IdProveedor);
    var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    var archivo = ExcelProveedor.Orden(local.Nombre, proveedor, orden);
    return Results.File(archivo, ExcelProveedor.TipoContenido, $"orden-{orden.Id}.xlsx");
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/proveedores/{id:int}/ordenes", async (int id, AppDbContext db, SesionActual sesion, int pagina = 1, int tamano = 20) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    tamano = Math.Clamp(tamano, 1, 50);
    pagina = Math.Max(1, pagina);
    var consulta = db.OrdenesCompra.Where(o => o.IdProveedor == id);
    var total = await consulta.CountAsync();
    var items = await consulta
        .OrderBy(o => o.FechaRecepcion != null).ThenByDescending(o => o.Fecha)
        .Skip((pagina - 1) * tamano).Take(tamano)
        .Select(o => new
        {
            o.Id,
            o.Fecha,
            o.NombreUsuario,
            cantidadLineas = o.Lineas.Count,
            recibida = o.FechaRecepcion != null,
            o.FechaRecepcion,
            o.NombreUsuarioRecepcion
        })
        .ToListAsync();
    return Results.Ok(new { total, pagina, tamano, items });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ordenes-compra/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    var orden = await db.OrdenesCompra.Include(o => o.Lineas).FirstOrDefaultAsync(o => o.Id == id);
    if (orden == null) return Results.NotFound();
    var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == orden.IdProveedor);
    return Results.Ok(new { orden = OrdenDto(orden), nombreProveedor = proveedor?.Nombre ?? "" });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/ordenes-compra/{id:int}/recibir", async (int id, RecibirOrdenPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarProveedores(sesion.Rol ?? "")) return Results.Forbid();
    if (pedido.Lineas.Count > 500) return Results.BadRequest("Máximo 500 líneas por orden.");
    var orden = await db.OrdenesCompra.Include(o => o.Lineas).FirstOrDefaultAsync(o => o.Id == id);
    if (orden == null) return Results.NotFound();
    try
    {
        var ids = orden.Lineas.Select(l => l.IdProducto).ToList();
        var productos = await db.Productos.Where(p => ids.Contains(p.Id)).ToListAsync();
        var recibidas = pedido.Lineas.Select(l => new LineaRecibida { IdLinea = l.IdLinea, Cantidad = l.Cantidad });
        var resultado = ServicioRecepcionOrden.Recibir(
            orden, recibidas, productos, sesion.IdUsuario, sesion.NombreUsuario ?? "sistema", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        return Results.Ok(new { orden = OrdenDto(orden), resultado.ProductosActualizados, resultado.LineasSinProducto });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict("La orden ya fue recibida.");
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/ventas", async (VentaPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.NroCaja == null) return Results.BadRequest("Elegí una caja.");
    var localVenta = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    if (!ServicioAbono.Estado(localVenta, DateTimeOffset.UtcNow).PuedeOperarCaja)
        return Results.Json("El local está vencido. Pagá el abono para seguir cobrando.", statusCode: StatusCodes.Status402PaymentRequired);
    if (pedido.CobrarAlCosto && !Permisos.PuedeCobrarAlCosto(sesion.Rol ?? ""))
        return Results.Forbid();
    if (await EsperaMercadoPago(db, pedido))
        return Results.BadRequest("Ese medio espera el pago de Mercado Pago.");
    var productos = await db.Productos.ToListAsync();
    var rigido = (await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == "StockRigido"))?.Valor == "1";
    try
    {
        var resultado = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = productos,
            Items = pedido.Items.Select(i => new ItemCarrito
            {
                IdProducto = i.IdProducto,
                Codigo = i.Codigo,
                Nombre = i.Nombre,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            DescuentoPorcentaje = pedido.DescuentoPorcentaje,
            CobrarAlCosto = pedido.CobrarAlCosto,
            StockRigido = rigido,
            MetodoPago = pedido.MetodoPago
        });

        var venta = new InformeVenta
        {
            IdLocal = sesion.IdLocal.Value,
            IdUsuario = sesion.IdUsuario,
            Fecha = DateTimeOffset.UtcNow,
            Total = resultado.Total,
            Subtotal = resultado.Subtotal,
            MetodoPago = resultado.MetodoPago,
            Descuento = resultado.Descuento,
            PrecioCosto = resultado.PrecioCosto,
            DetalleAdjunto = true,
            NroCaja = sesion.NroCaja.Value
        };
        var pagosSolicitados = pedido.Pagos.Count > 0
            ? pedido.Pagos.Select(p => new PagoSolicitado(p.IdMetodoPago, p.Importe)).ToList()
            : await MetodoUnico(db, pedido.MetodoPago, resultado.Total);
        ServicioPagos.Validar(resultado.Total, pagosSolicitados);
        var idsMedios = pagosSolicitados.Select(p => p.IdMetodoPago).Distinct().ToList();
        var medios = await db.MetodosPago.Where(m => idsMedios.Contains(m.Id) && m.Activo).ToDictionaryAsync(m => m.Id);
        if (medios.Count != idsMedios.Count)
            throw new ErrorNegocio("El medio de pago no existe o está inactivo.");
        venta.MetodoPago = string.Join(", ", pagosSolicitados.Select(p => medios[p.IdMetodoPago].Descripcion).Distinct());
        venta.MultipleMetodoDePago = pagosSolicitados.Select(p => p.IdMetodoPago).Distinct().Count() > 1;
        foreach (var l in resultado.Lineas)
        {
            venta.Detalles.Add(new InformeVentaDetalle
            {
                IdLocal = sesion.IdLocal.Value,
                IdProducto = l.IdProducto == 0 ? null : l.IdProducto,
                Codigo = l.Codigo,
                Nombre = l.Nombre,
                Cantidad = l.Cantidad,
                Precio = l.Precio,
                Costo = l.Costo,
                SubTotal = l.SubTotal
            });
        }
        venta.Pagos.AddRange(pagosSolicitados.Select(p => new PagoVenta
        {
            IdLocal = sesion.IdLocal.Value,
            IdInformeVenta = venta.IdInformeVenta,
            IdMetodoPago = p.IdMetodoPago,
            DescripcionMetodoPago = medios[p.IdMetodoPago].Descripcion,
            Importe = p.Importe
        }));
        db.InformeVenta.Add(venta);
        await db.SaveChangesAsync();
        return Results.Created($"/ventas/{venta.IdInformeVenta}", new { venta.IdInformeVenta, venta.Total });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/mercadopago/cuenta", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var vinculada = await db.CuentasMercadoPago.AnyAsync(c => c.IdLocal == sesion.IdLocal);
    var puestos = await db.PuestosMercadoPago.OrderBy(p => p.NroCaja)
        .Select(p => new { p.NroCaja, listo = p.IdExterno != "", p.UrlQr, p.IdLector })
        .ToListAsync();
    return Results.Ok(new { vinculada, puestos });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/mercadopago/conectar", (SesionActual sesion, IDataProtectionProvider proteccion, IConfiguration config) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    try
    {
        var clientId = CobrosMercadoPagoApp.Exigir(config, "MercadoPago:ClientId");
        var redirect = RedirectMercadoPago(config);
        var state = proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito)
            .Protect($"{sesion.IdLocal}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
        return Results.Ok(new { url = ClienteMercadoPagoLocal.UrlAutorizacion(clientId, redirect, state) });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/mercadopago/vuelta", async (string? code, string? state, AppDbContext db, ClienteMercadoPagoLocal mp, IDataProtectionProvider proteccion, IConfiguration config, ILoggerFactory logs) =>
{
    var web = string.IsNullOrWhiteSpace(config["App:UrlWeb"]) ? "http://localhost:4200" : config["App:UrlWeb"]!.TrimEnd('/');
    var error = Results.Redirect($"{web}/configuracion?mp=error");
    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state)) return error;
    try
    {
        var datos = proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito).Unprotect(state).Split('|');
        if (datos.Length != 2 || !int.TryParse(datos[0], out var idLocal) || !long.TryParse(datos[1], out var ticks))
            return error;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ticks > 900) return error;
        var token = await mp.CanjearCodigo(
            CobrosMercadoPagoApp.Exigir(config, "MercadoPago:ClientId"),
            CobrosMercadoPagoApp.Exigir(config, "MercadoPago:ClientSecret"),
            code,
            RedirectMercadoPago(config));
        var protector = proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito);
        var cuenta = await db.CuentasMercadoPago.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.IdLocal == idLocal);
        if (cuenta == null)
        {
            cuenta = new CuentaMercadoPago { IdLocal = idLocal };
            db.CuentasMercadoPago.Add(cuenta);
        }
        CobrosMercadoPagoApp.GuardarToken(protector, cuenta, token);
        await db.SaveChangesAsync();
        return Results.Redirect($"{web}/configuracion?mp=ok");
    }
    catch (Exception ex)
    {
        logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo vincular la cuenta del local");
        return error;
    }
}).RequireRateLimiting("api");

app.MapPost("/mercadopago/puestos/{nro:int}", async (int nro, AppDbContext db, SesionActual sesion, ClienteMercadoPagoLocal mp, IDataProtectionProvider proteccion, IConfiguration config, ILoggerFactory logs) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    if (nro < 1 || nro > await CantidadCajasLocal(db)) return Results.BadRequest("Ese puesto no existe.");
    try
    {
        var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
        await CobrosMercadoPagoApp.PrepararPuesto(db, mp, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config, sesion.IdLocal.Value, nro, local.Nombre);
        var puesto = await db.PuestosMercadoPago.FirstAsync(p => p.NroCaja == nro);
        return Results.Ok(new { puesto.NroCaja, listo = true, puesto.UrlQr, puesto.IdLector });
    }
    catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
    catch (Exception ex)
    {
        logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo preparar el puesto {Nro}", nro);
        return Results.Json("Mercado Pago no pudo preparar el puesto.", statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/mercadopago/puestos/{nro:int}", async (int nro, LectorMercadoPagoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
    if (nro < 1 || nro > await CantidadCajasLocal(db)) return Results.BadRequest("Ese puesto no existe.");
    await CobrosMercadoPagoApp.GuardarLector(db, sesion.IdLocal.Value, nro, pedido.IdLector ?? "");
    return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/ventas/mercadopago", async (VentaPedido pedido, AppDbContext db, SesionActual sesion, ClienteMercadoPagoLocal mp, IDataProtectionProvider proteccion, IConfiguration config, ILoggerFactory logs) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.NroCaja == null) return Results.BadRequest("Elegí una caja.");
    var localVenta = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    if (!ServicioAbono.Estado(localVenta, DateTimeOffset.UtcNow).PuedeOperarCaja)
        return Results.Json("El local está vencido. Pagá el abono para seguir cobrando.", statusCode: StatusCodes.Status402PaymentRequired);
    if (pedido.CobrarAlCosto && !Permisos.PuedeCobrarAlCosto(sesion.Rol ?? ""))
        return Results.Forbid();
    try
    {
        var productos = await db.Productos.AsNoTracking().ToListAsync();
        var rigido = (await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == "StockRigido"))?.Valor == "1";
        var resultado = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = productos,
            Items = pedido.Items.Select(i => new ItemCarrito
            {
                IdProducto = i.IdProducto,
                Codigo = i.Codigo,
                Nombre = i.Nombre,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            DescuentoPorcentaje = pedido.DescuentoPorcentaje,
            CobrarAlCosto = pedido.CobrarAlCosto,
            StockRigido = rigido
        });
        var pagos = pedido.Pagos.Count > 0
            ? pedido.Pagos.Select(p => new PagoSolicitado(p.IdMetodoPago, p.Importe)).ToList()
            : await MetodoUnico(db, pedido.MetodoPago, resultado.Total);
        var cobro = await CobrosMercadoPagoApp.Iniciar(
            db, mp, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config,
            sesion.IdLocal.Value, sesion.NroCaja.Value, sesion.IdUsuario,
            new PedidoGuardado
            {
                Items = pedido.Items.Select(i => new ItemCarrito
                {
                    IdProducto = i.IdProducto,
                    Codigo = i.Codigo,
                    Nombre = i.Nombre,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList(),
                Pagos = pagos,
                DescuentoPorcentaje = pedido.DescuentoPorcentaje,
                CobrarAlCosto = pedido.CobrarAlCosto
            },
            resultado.Total);
        return Results.Ok(VistaCobro(cobro));
    }
    catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
    catch (Exception ex)
    {
        logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo crear la orden del local {IdLocal}", sesion.IdLocal);
        return Results.Json("Mercado Pago no pudo crear el cobro.", statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ventas/mercadopago/abierto", async (AppDbContext db, SesionActual sesion, ClienteMercadoPagoLocal mp, IDataProtectionProvider proteccion, IConfiguration config) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.NroCaja == null) return Results.BadRequest("Elegí una caja.");
    var cobro = await CobrosMercadoPagoApp.Abierto(
        db, mp, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config,
        sesion.IdLocal.Value, sesion.NroCaja.Value);
    return cobro == null ? Results.NoContent() : Results.Ok(VistaCobro(cobro));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ventas/mercadopago/{id:int}", async (int id, AppDbContext db, SesionActual sesion, ClienteMercadoPagoLocal mp, IDataProtectionProvider proteccion, IConfiguration config) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    try
    {
        var cobro = await CobrosMercadoPagoApp.Consultar(db, mp, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config, id, sesion.IdLocal.Value);
        return cobro == null ? Results.NotFound() : Results.Ok(VistaCobro(cobro));
    }
    catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
    catch (Exception)
    {
        return Results.Json("No se pudo consultar el cobro.", statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/ventas/mercadopago/{id:int}/cancelar", async (int id, AppDbContext db, SesionActual sesion, ClienteMercadoPagoLocal mp, IDataProtectionProvider proteccion, IConfiguration config) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    try
    {
        var cobro = await CobrosMercadoPagoApp.Cancelar(db, mp, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config, id, sesion.IdLocal.Value);
        return cobro == null ? Results.NotFound() : Results.Ok(VistaCobro(cobro));
    }
    catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
    catch (Exception)
    {
        return Results.Json("No se pudo cancelar el cobro.", statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ventas", async (AppDbContext db, SesionActual sesion, DateOnly? desde, DateOnly? hasta, string? medio, int? nroCaja, int? idCierre, int pagina = 1, int tamano = 50) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    tamano = Math.Clamp(tamano, 1, maxPagina);
    pagina = Math.Max(1, pagina);
    IQueryable<InformeVenta> q = db.InformeVenta;
    if (idCierre is > 0)
        q = q.Where(v => v.IdCierre == idCierre);
    else
    {
        var diaDesde = desde ?? DiaArgentina.Hoy();
        var diaHasta = hasta ?? diaDesde;
        var (ini, _) = DiaArgentina.Rango(diaDesde);
        var (_, fin) = DiaArgentina.Rango(diaHasta);
        q = q.Where(v => v.Fecha >= ini && v.Fecha < fin);
    }
    if (nroCaja is > 0)
        q = q.Where(v => v.NroCaja == nroCaja);
    if (sesion.Rol == Roles.Empleado)
        q = q.Where(v => v.IdUsuario == sesion.IdUsuario);
    if (!string.IsNullOrWhiteSpace(medio))
        q = q.Where(v => v.MetodoPago == medio || db.PagosVenta.Any(p => p.IdInformeVenta == v.IdInformeVenta && p.DescripcionMetodoPago == medio));
    q = q.OrderByDescending(v => v.Fecha);
    var total = await q.CountAsync();
    var suma = await q.SumAsync(v => (decimal?)v.Total) ?? 0;
    var items = await q.Skip((pagina - 1) * tamano).Take(tamano)
        .Select(v => new { v.IdInformeVenta, v.Fecha, v.Total, v.MetodoPago, v.Descuento, v.PrecioCosto, v.NroCaja, v.IdCierre })
        .ToListAsync();
    return Results.Ok(new { total, suma, pagina, tamano, items });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ventas/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var venta = await db.InformeVenta.Include(v => v.Detalles).Include(v => v.Pagos).FirstOrDefaultAsync(v => v.IdInformeVenta == id);
    if (venta == null) return Results.NotFound();
    if (sesion.Rol == Roles.Empleado && venta.IdUsuario != sesion.IdUsuario) return Results.NotFound();
    var verCosto = Permisos.PuedeVerCostoEnInforme(sesion.Rol ?? "");
    return Results.Ok(new
    {
        venta.IdInformeVenta,
        venta.Fecha,
        venta.Total,
        venta.Subtotal,
        venta.MetodoPago,
        venta.Descuento,
        venta.PrecioCosto,
        venta.NroCaja,
        venta.IdCierre,
        pagos = venta.Pagos.Select(p => new { p.IdMetodoPago, p.DescripcionMetodoPago, p.Importe }),
        items = venta.Detalles.Select(d => new
        {
            d.IdInformeVentaDetalle,
            d.IdProducto,
            d.Codigo,
            d.Nombre,
            d.Cantidad,
            d.Precio,
            costo = verCosto ? d.Costo : null,
            d.SubTotal
        })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapDelete("/ventas/{id:int}", async (int id, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAnularVenta(sesion.Rol ?? "")) return Results.Forbid();
    var venta = await db.InformeVenta.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.IdInformeVenta == id);
    if (venta == null) return Results.NotFound();
    var productos = await db.Productos.ToListAsync();
    try
    {
        ServicioAnularVenta.Anular(venta, productos, sesion.NombreUsuario ?? "sistema");
        db.InformeVenta.Remove(venta);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/estadisticas", async (
    AppDbContext db,
    SesionActual sesion,
    int? idLocal,
    bool? todos,
    DateOnly? desde,
    DateOnly? hasta,
    string? mes) =>
{
    if (!Permisos.PuedeVerEstadisticas(sesion.Rol ?? ""))
        return Results.Forbid();

    var accesibles = await LocalesAccesibles(db, sesion);
    if (accesibles.Count == 0)
        return Results.Ok(new { resumen = new ResumenEstadisticas(), periodo = new { }, locales = Array.Empty<object>() });

    List<int> localesObjetivo;
    if (todos == true)
    {
        localesObjetivo = accesibles;
    }
    else if (idLocal.HasValue)
    {
        if (!accesibles.Contains(idLocal.Value))
            return Results.Forbid();
        localesObjetivo = [idLocal.Value];
    }
    else if (sesion.IdLocal.HasValue && accesibles.Contains(sesion.IdLocal.Value))
    {
        localesObjetivo = [sesion.IdLocal.Value];
    }
    else
    {
        localesObjetivo = [accesibles[0]];
    }

    var hoy = DiaArgentina.Hoy();
    DateTimeOffset ini;
    DateTimeOffset fin;

    if (!string.IsNullOrWhiteSpace(mes) && DateTime.TryParseExact(mes, ["yyyy-MM", "yyyy-M"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaMes))
    {
        var primerDiaMes = new DateOnly(fechaMes.Year, fechaMes.Month, 1);
        var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);
        (ini, _) = DiaArgentina.Rango(primerDiaMes);
        (_, fin) = DiaArgentina.Rango(ultimoDiaMes);
    }
    else if (desde.HasValue || hasta.HasValue)
    {
        var diaDesde = desde ?? hoy;
        var diaHasta = hasta ?? diaDesde;
        if (diaHasta < diaDesde) (diaDesde, diaHasta) = (diaHasta, diaDesde);
        (ini, _) = DiaArgentina.Rango(diaDesde);
        (_, fin) = DiaArgentina.Rango(diaHasta);
    }
    else
    {
        var primerDiaMes = new DateOnly(hoy.Year, hoy.Month, 1);
        (ini, _) = DiaArgentina.Rango(primerDiaMes);
        (_, fin) = DiaArgentina.Rango(hoy);
    }

    var (iniHoy, finHoy) = DiaArgentina.Rango(hoy);

    var ventasPeriodo = await db.InformeVenta
        .IgnoreQueryFilters()
        .Where(v => localesObjetivo.Contains(v.IdLocal) && v.Fecha >= ini && v.Fecha < fin)
        .Include(v => v.Detalles)
        .Include(v => v.Pagos)
        .ToListAsync();

    List<InformeVenta> ventasHoy;
    if (ini == iniHoy && fin == finHoy)
    {
        ventasHoy = ventasPeriodo;
    }
    else
    {
        ventasHoy = await db.InformeVenta
            .IgnoreQueryFilters()
            .Where(v => localesObjetivo.Contains(v.IdLocal) && v.Fecha >= iniHoy && v.Fecha < finHoy)
            .Include(v => v.Detalles)
            .ToListAsync();
    }

    var productos = await db.Productos
        .IgnoreQueryFilters()
        .Where(p => localesObjetivo.Contains(p.IdLocal))
        .ToListAsync();

    var grupos = await db.GrupoProductos
        .IgnoreQueryFilters()
        .Where(g => localesObjetivo.Contains(g.IdLocal))
        .ToListAsync();

    var locales = await db.Locales
        .Where(l => accesibles.Contains(l.IdLocal))
        .ToListAsync();

    var configsUmbral = await db.Configuracion
        .IgnoreQueryFilters()
        .Where(c => localesObjetivo.Contains(c.IdLocal) && c.Clave == "UmbralStockBajo")
        .ToListAsync();

    var umbralesPorLocal = localesObjetivo.ToDictionary(
        id => id,
        id =>
        {
            var conf = configsUmbral.FirstOrDefault(c => c.IdLocal == id);
            return decimal.TryParse(conf?.Valor, out var u) && u >= 0 ? u : 5m;
        });

    var configsFactor = await db.Configuracion
        .IgnoreQueryFilters()
        .Where(c => localesObjetivo.Contains(c.IdLocal)
            && (c.Clave == "FactorGanancia" || c.Clave == "FactorGananciaSector" || c.Clave == "FactorGananciaGenerico"))
        .ToListAsync();
    var factoresPorLocal = localesObjetivo.ToDictionary(
        id => id,
        id => new FactoresGananciaLibre(
            FactorLibre(configsFactor, id, "FactorGananciaSector"),
            FactorLibre(configsFactor, id, "FactorGananciaGenerico"),
            FactorLibre(configsFactor, id, "FactorGanancia")));

    var resumen = ServicioEstadisticas.Calcular(
        ventasPeriodo,
        ventasHoy,
        productos,
        grupos,
        locales,
        umbralesPorLocal,
        factoresPorLocal: factoresPorLocal);

    return Results.Ok(new
    {
        resumen,
        periodo = new
        {
            desde = ini,
            hasta = fin,
            hoy = iniHoy
        },
        locales = locales.Select(l => new { l.IdLocal, l.Nombre })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/cajas", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.NroCaja == null) return Results.BadRequest("Elegí una caja.");
    var (ini, fin) = DiaArgentina.Rango(DiaArgentina.Hoy());
    var puesto = sesion.NroCaja.Value;
    var abiertas = db.InformeVenta.Where(v =>
        v.Fecha >= ini && v.Fecha < fin && v.IdCierre == null && v.NroCaja == puesto);
    var pendientesHoy = new
    {
        tickets = await abiertas.CountAsync(),
        total = await abiertas.SumAsync(v => (decimal?)v.Total) ?? 0
    };
    var items = await db.Cajas.Where(c => c.MetodoPago == "Total")
        .OrderByDescending(c => c.IdCierre)
        .Take(20)
        .Select(c => new { c.IdCierre, c.NroCaja, c.Fecha, c.NombreCierre, c.Total, c.CantidadVentas })
        .ToListAsync();
    return Results.Ok(new { nroCaja = puesto, pendientesHoy, items });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/cajas/abiertas", async (AppDbContext db, SesionActual sesion, DateOnly? fecha) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.EsDuenoOperativo(sesion.Rol ?? "")) return Results.Forbid();
    var (ini, fin) = DiaArgentina.Rango(fecha ?? DiaArgentina.Hoy());
    var cajas = await db.InformeVenta
        .Where(v => v.Fecha >= ini && v.Fecha < fin && v.IdCierre == null)
        .GroupBy(v => v.NroCaja)
        .Select(g => new { nroCaja = g.Key, tickets = g.Count(), total = g.Sum(v => v.Total) })
        .OrderBy(c => c.nroCaja)
        .ToListAsync();
    return Results.Ok(cajas);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/configuracion/cajas", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var cantidad = await CantidadCajasLocal(db);
    return Results.Ok(new { cantidad });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPut("/configuracion/cajas", async (CantidadCajasPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeConfigurarCajas(sesion.Rol ?? "")) return Results.Forbid();
    if (pedido.Cantidad < 1) return Results.BadRequest("La cantidad mínima es 1.");
    var fila = await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == "CantidadCajas");
    if (fila == null)
    {
        fila = new Configuracion { IdLocal = sesion.IdLocal.Value, Clave = "CantidadCajas", Valor = pedido.Cantidad.ToString() };
        db.Configuracion.Add(fila);
    }
    else
        fila.Valor = pedido.Cantidad.ToString();
    await db.SaveChangesAsync();
    return Results.Ok(new { cantidad = pedido.Cantidad });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/cajas/puestos", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var cantidad = await CantidadCajasLocal(db);
    var ocupaciones = await db.OcupacionesCaja
        .OrderBy(o => o.NroCaja)
        .Select(o => new { o.NroCaja, o.NombreUsuario, o.IdUsuario })
        .ToListAsync();
    return Results.Ok(new { cantidad, ocupaciones });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/cajas/puestos/{nro:int}/elegir", async (int nro, AppDbContext db, SesionActual sesion, JwtServicio jwt) =>
{
    if (sesion.IdLocal == null || sesion.IdUsuario == null) return Results.BadRequest("Elegí un local.");
    var cantidad = await CantidadCajasLocal(db);
    if (nro < 1 || nro > cantidad)
        return Results.BadRequest($"Elegí una caja entre 1 y {cantidad}.");
    var u = await db.Usuarios.IgnoreQueryFilters().FirstAsync(x => x.Id == sesion.IdUsuario);
    var otros = await db.OcupacionesCaja
        .Where(o => o.NroCaja == nro && o.IdUsuario != sesion.IdUsuario)
        .Select(o => o.NombreUsuario)
        .ToListAsync();
    var mias = await db.OcupacionesCaja.Where(o => o.IdUsuario == sesion.IdUsuario).ToListAsync();
    db.OcupacionesCaja.RemoveRange(mias);
    db.OcupacionesCaja.Add(new OcupacionCaja
    {
        IdLocal = sesion.IdLocal.Value,
        NroCaja = nro,
        IdUsuario = sesion.IdUsuario.Value,
        NombreUsuario = u.NombreUsuario,
        Desde = DateTimeOffset.UtcNow
    });
    await db.SaveChangesAsync();
    string? aviso = otros.Count == 0
        ? null
        : $"{string.Join(", ", otros)} está usando la Caja {nro}";
    return Results.Ok(new
    {
        token = jwt.Emitir(u, sesion.IdLocal, nro),
        nroCaja = nro,
        aviso
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/cajas/filtros", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var personas = await db.Cajas.Where(c => c.MetodoPago == "Total" && c.NombreCierre != "")
        .Select(c => c.NombreCierre)
        .Distinct()
        .OrderBy(n => n)
        .ToListAsync();
    var medios = await db.Cajas.Where(c => c.MetodoPago != "Total" && c.TipoDesglose == TipoDesgloseCaja.Medio)
        .Select(c => c.MetodoPago)
        .Distinct()
        .OrderBy(m => m)
        .ToListAsync();
    return Results.Ok(new { personas, medios });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/cajas/consulta", async (
    AppDbContext db,
    SesionActual sesion,
    DateOnly? desde,
    DateOnly? hasta,
    string? quien,
    string? medio,
    int pagina = 1,
    int tamano = 50) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    tamano = Math.Clamp(tamano, 1, maxPagina);
    pagina = Math.Max(1, pagina);
    var hoy = DiaArgentina.Hoy();
    var (mesIni, mesFin) = ServicioConsultaCajas.MesActual(hoy);
    var diaDesde = desde ?? mesIni;
    var diaHasta = hasta ?? mesFin;
    if (diaHasta < diaDesde)
        return Results.BadRequest("La fecha hasta no puede ser anterior a desde.");
    var (ini, _) = DiaArgentina.Rango(diaDesde);
    var (_, fin) = DiaArgentina.Rango(diaHasta);
    var enRango = await db.Cajas.Where(c => c.Fecha >= ini && c.Fecha < fin).ToListAsync();
    var filtradas = ServicioConsultaCajas.FiltrarTotales(enRango, quien, medio);
    var total = filtradas.Count;
    var items = filtradas
        .Skip((pagina - 1) * tamano)
        .Take(tamano)
        .Select(c => new { c.IdCierre, c.NroCaja, c.Fecha, c.NombreCierre, c.Total, c.CantidadVentas })
        .ToList();
    var paraUnificar = string.IsNullOrWhiteSpace(medio)
        ? filtradas
        : ServicioConsultaCajas.FiltrarTotales(enRango, quien, null);
    var unificables = ServicioUnificarCajas.GruposUnificables(paraUnificar)
        .Select(g => new { g.NroCaja, g.NombreCierre, dia = g.Dia.ToString("yyyy-MM-dd"), idsCierre = g.IdsCierre })
        .ToList();
    return Results.Ok(new { total, pagina, tamano, items, unificables });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/cajas/unificar", async (UnificarCajasPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (pedido.IdsCierre == null || pedido.IdsCierre.Count < 2)
        return Results.BadRequest("Elegí al menos dos cierres.");
    var ids = pedido.IdsCierre.Distinct().ToList();
    var filas = await db.Cajas.Where(c => ids.Contains(c.IdCierre)).ToListAsync();
    try
    {
        var r = ServicioUnificarCajas.Unificar(filas, ids);
        db.Cajas.RemoveRange(filas);
        db.Cajas.AddRange(r.Filas);
        var ventas = await db.InformeVenta.Where(v => v.IdCierre != null && ids.Contains(v.IdCierre.Value)).ToListAsync();
        foreach (var v in ventas)
            v.IdCierre = r.IdCierre;
        await db.SaveChangesAsync();
        return Results.Ok(new
        {
            r.IdCierre,
            eliminados = r.IdsEliminados,
            filas = r.Filas.Select(c => new { c.MetodoPago, c.CantidadVentas, c.Total })
        });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/cajas/cierres/{idCierre:int}", async (int idCierre, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    var filas = await db.Cajas.Where(c => c.IdCierre == idCierre).OrderBy(c => c.IdCaja).ToListAsync();
    if (filas.Count == 0) return Results.NotFound();
    return Results.Ok(new
    {
        idCierre,
        nroCaja = filas[0].NroCaja,
        fecha = filas[0].Fecha,
        nombreCierre = filas[0].NombreCierre,
        tipoDesglose = filas[0].TipoDesglose,
        filas = filas.Select(c => new { c.MetodoPago, c.CantidadVentas, c.Total })
    });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/cajas/{nro:int}", async (int nro, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    // Compat: si existe IdCierre = nro, devolver ese cierre; si no, último Total del puesto.
    var porId = await db.Cajas.Where(c => c.IdCierre == nro).OrderBy(c => c.IdCaja).ToListAsync();
    if (porId.Count > 0)
    {
        return Results.Ok(new
        {
            idCierre = nro,
            nroCaja = porId[0].NroCaja,
            fecha = porId[0].Fecha,
            nombreCierre = porId[0].NombreCierre,
            tipoDesglose = porId[0].TipoDesglose,
            filas = porId.Select(c => new { c.MetodoPago, c.CantidadVentas, c.Total })
        });
    }
    return Results.NotFound();
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/cajas/cerrar", async (CerrarCajaPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.NroCaja == null) return Results.BadRequest("Elegí una caja.");
    var dia = pedido.Fecha ?? DiaArgentina.Hoy();
    var (ini, fin) = DiaArgentina.Rango(dia);
    var ventas = await db.InformeVenta.Where(v => v.Fecha >= ini && v.Fecha < fin).ToListAsync();
    var idsVentas = ventas.Select(v => v.IdInformeVenta).ToList();
    var pagosPorVenta = await db.PagosVenta.Where(p => idsVentas.Contains(p.IdInformeVenta))
        .GroupBy(p => p.IdInformeVenta)
        .ToDictionaryAsync(g => g.Key, g => (IReadOnlyList<PagoVenta>)g.ToList());
    var puedeCerrarTodas = Permisos.EsDuenoOperativo(sesion.Rol ?? "");
    var puestosAbiertos = ventas.Where(v => v.IdCierre == null).Select(v => v.NroCaja).Distinct().OrderBy(n => n).ToList();
    var puestos = pedido.Todas && puedeCerrarTodas
        ? puestosAbiertos
        : puestosAbiertos.Where(n => n == sesion.NroCaja.Value).ToList();
    var idCierre = (await db.Cajas.MaxAsync(c => (int?)c.IdCierre) ?? 0) + 1;
    var nombre = sesion.IdUsuario == null
        ? ""
        : (await db.Usuarios.IgnoreQueryFilters().FirstAsync(u => u.Id == sesion.IdUsuario)).NombreUsuario;
    var idsUsuarios = ventas.Where(v => v.IdUsuario != null).Select(v => v.IdUsuario!.Value).Distinct().ToList();
    var nombres = await db.Usuarios.IgnoreQueryFilters()
        .Where(u => idsUsuarios.Contains(u.Id))
        .ToDictionaryAsync(u => u.Id, u => u.NombreUsuario);
    var tipo = string.Equals(pedido.Desglose, "usuario", StringComparison.OrdinalIgnoreCase)
        ? TipoDesgloseCaja.Usuario
        : TipoDesgloseCaja.Medio;
    try
    {
        var resultados = new List<ResultadoCierre>();
        foreach (var puesto in puestos)
        {
            var r = ServicioCierre.Cerrar(
                ventas,
                puesto,
                idCierre++,
                sesion.IdLocal.Value,
                sesion.IdUsuario,
                nombre,
                DateTimeOffset.UtcNow,
                tipo,
                nombres,
                pagosPorVenta);
            resultados.Add(r);
            db.Cajas.AddRange(r.Filas);
        }
        if (resultados.Count == 0)
            throw new ErrorNegocio("No hay ventas para cerrar en ese día.");
        await db.SaveChangesAsync();
        var primero = resultados[0];
        return Results.Ok(new
        {
            idCierre = primero.IdCierre,
            nroCaja = primero.NroCaja,
            filas = primero.Filas.Select(c => new { c.MetodoPago, c.CantidadVentas, c.Total }),
            cierres = resultados.Select(r => new { r.IdCierre, r.NroCaja })
        });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.Run();

static async Task<int> CopiarCatalogoAlLocal(AppDbContext db, int idDueno, int idLocalDestino, AltaLocalAdminPedido pedido, string usuario)
{
    if (pedido.IdLocalOrigen <= 0) return 0;
    var propio = await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == idDueno && x.IdLocal == pedido.IdLocalOrigen);
    if (!propio) throw new ErrorNegocio("Ese local no es de este dueño.");

    var idOrigen = pedido.IdLocalOrigen;
    var grupos = await db.GrupoProductos.IgnoreQueryFilters().Where(g => g.IdLocal == idOrigen).ToListAsync();
    var proveedores = await db.Proveedores.IgnoreQueryFilters().Where(p => p.IdLocal == idOrigen).ToListAsync();
    var productos = await db.Productos.IgnoreQueryFilters().Where(p => p.IdLocal == idOrigen).ToListAsync();
    var copia = ServicioAdmin.CopiarCatalogo(grupos, proveedores, productos, idLocalDestino, pedido.CopiarStock, DateTimeOffset.UtcNow, usuario);
    db.GrupoProductos.AddRange(copia.Grupos);
    db.Proveedores.AddRange(copia.Proveedores);
    await db.SaveChangesAsync();
    foreach (var item in copia.Productos)
    {
        if (copia.GrupoPorIdOrigen.TryGetValue(item.IdGrupoOrigen, out var grupo))
            item.Producto.IdGrupoProducto = grupo.IdGrupoProducto;
        if (copia.ProveedorPorIdOrigen.TryGetValue(item.IdProveedorOrigen, out var proveedor))
            item.Producto.IdProveedor = proveedor.Id;
        db.Productos.Add(item.Producto);
    }
    return copia.Productos.Count;
}

static async Task<IResult> EmitirPagoContratacion(Local local, AppDbContext db, ClienteMercadoPago mp, ILoggerFactory logs, IConfiguration config)
{
    var ahora = DateTimeOffset.UtcNow;
    var vigente = await PreferenciaContratacion(db, local.IdLocal, ahora);
    if (vigente != null) return Results.Ok(new { local.IdLocal, local.Nombre, urlPago = vigente.UrlPago });

    var abono = ServicioAbono.NuevoAbono(local.IdLocal, Abono.OrigenMp, ahora);
    db.Abonos.Add(abono);
    await db.SaveChangesAsync();
    try
    {
        var web = config["App:UrlWeb"];
        if (string.IsNullOrWhiteSpace(web)) throw new InvalidOperationException("Falta App:UrlWeb.");
        var preferencia = await mp.CrearPreferencia(abono, local.Nombre, $"{web.TrimEnd('/')}/locales");
        abono.IdPreferenciaMp = preferencia.Id;
        abono.UrlPago = preferencia.UrlPago;
        await db.SaveChangesAsync();
        return Results.Ok(new { local.IdLocal, local.Nombre, urlPago = abono.UrlPago });
    }
    catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
    {
        logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo crear la preferencia del local {IdLocal}", local.IdLocal);
        abono.Estado = Abono.Rechazado;
        await db.SaveChangesAsync();
        return Results.Json(ErrorMercadoPago, statusCode: StatusCodes.Status502BadGateway);
    }
}

static async Task<Abono?> PreferenciaContratacion(AppDbContext db, int idLocal, DateTimeOffset ahora)
{
    var desde = ahora.AddMinutes(-30);
    return await db.Abonos.IgnoreQueryFilters()
        .Where(a => a.IdLocal == idLocal && a.Estado == Abono.Pendiente && a.Origen == Abono.OrigenMp && a.UrlPago != null && a.FechaCreacion > desde)
        .OrderByDescending(a => a.FechaCreacion)
        .FirstOrDefaultAsync();
}

static async Task<Abono?> AbonoPendiente(AppDbContext db, DateTimeOffset ahora)
{
    var desde = ahora.AddMinutes(-30);
    return await db.Abonos
        .Where(a => a.Estado == Abono.Pendiente && a.Origen == Abono.OrigenMp && a.UrlPago != null && a.FechaCreacion > desde)
        .OrderByDescending(a => a.FechaCreacion)
        .FirstOrDefaultAsync();
}

static async Task<int> CantidadCajasLocal(AppDbContext db)
{
    var valor = (await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == "CantidadCajas"))?.Valor;
    return int.TryParse(valor, out var n) && n >= 1 ? n : 1;
}

static async Task<ConfiguracionLocalPedido> LeerConfiguracionLocal(AppDbContext db, Local local)
{
    var filas = await db.Configuracion.ToListAsync();
    var valores = filas.ToDictionary(c => c.Clave, c => c.Valor, StringComparer.OrdinalIgnoreCase);
    var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["FactorGanancia"] = "1",
        ["FactorGananciaSector"] = "1",
        ["FactorGananciaGenerico"] = "1",
        ["IVA"] = "1.21",
        ["StockRigido"] = "0",
        ["UmbralStockBajo"] = "5",
        ["EmpleadoPuedeModificarPrecios"] = "0",
        ["FormatoTicket"] = "POS-80",
        ["ImprimirTicketAlCobrar"] = "1",
        ["CantidadCajas"] = "1"
    };
    foreach (var (clave, valor) in defaults)
    {
        if (valores.ContainsKey(clave)) continue;
        var fila = new Configuracion { IdLocal = local.IdLocal, Clave = clave, Valor = valor };
        db.Configuracion.Add(fila);
        valores[clave] = valor;
    }
    return new ConfiguracionLocalPedido
    {
        NombreLocal = local.Nombre,
        FactorGanancia = DecimalConfig(valores["FactorGanancia"], 1),
        FactorGananciaSector = DecimalConfig(valores["FactorGananciaSector"], 1),
        FactorGananciaGenerico = DecimalConfig(valores["FactorGananciaGenerico"], 1),
        Iva = DecimalConfig(valores["IVA"], 1.21m),
        StockRigido = BoolConfig(valores["StockRigido"]),
        UmbralStockBajo = DecimalConfig(valores["UmbralStockBajo"], 5),
        EmpleadoPuedeModificarPrecios = BoolConfig(valores["EmpleadoPuedeModificarPrecios"]),
        FormatoTicket = ServicioTicket.FormatoODefault(valores["FormatoTicket"]),
        ImprimirTicketAlCobrar = BoolConfig(valores["ImprimirTicketAlCobrar"]),
        CantidadCajas = IntConfig(valores["CantidadCajas"], 1)
    };
}

static string? DataUrl(PlantillaCartel? plantilla) =>
    plantilla == null ? null : $"data:{plantilla.TipoContenido};base64,{Convert.ToBase64String(plantilla.Datos)}";

static byte[] DecodificarBase64(string imagen)
{
    var texto = imagen ?? "";
    var coma = texto.IndexOf(',');
    if (texto.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && coma > 0)
        texto = texto[(coma + 1)..];
    try
    {
        return Convert.FromBase64String(texto.Trim());
    }
    catch (FormatException)
    {
        throw new ErrorNegocio("El archivo no es válido.");
    }
}

static async Task<ConfiguracionLocalPedido> ConfiguracionDelLocal(AppDbContext db, SesionActual sesion)
{
    var local = await db.Locales.FirstAsync(l => l.IdLocal == sesion.IdLocal);
    return await LeerConfiguracionLocal(db, local);
}

static async Task<List<Producto>> ProductosPorCodigo(AppDbContext db, IEnumerable<FilaCatalogo> filas)
{
    var codigos = filas.Select(f => f.Codigo.Trim()).Where(c => c.Length > 0).Distinct().ToList();
    return await db.Productos.Where(p => codigos.Contains(p.Codigo)).ToListAsync();
}

static async Task<string> TelefonoVentas(AppDbContext db) =>
    await db.Ajustes.AsNoTracking()
        .Where(a => a.Clave == Ajuste.TelefonoVentas)
        .Select(a => a.Valor)
        .FirstOrDefaultAsync() ?? "";

static async Task<bool> EnviarConfirmacion(IHttpClientFactory clientes, string apiKey, string desde, string para, string enlace)
{
    var req = new HttpRequestMessage(HttpMethod.Post, "emails");
    req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + apiKey);
    req.Content = JsonContent.Create(new
    {
        from = desde,
        to = new[] { para },
        subject = "Confirmá tu cuenta de StockControl",
        text = "Abrí este enlace para activar la cuenta. Vence en 48 horas.\n\n" + enlace + "\n"
    });
    using var resp = await clientes.CreateClient("resend").SendAsync(req);
    return resp.IsSuccessStatusCode;
}

static (string SiteKey, string Secreto)? ClavesCaptcha(IConfiguration config, IWebHostEnvironment env)
{
    var site = config["Turnstile:SiteKey"];
    var secreto = config["Turnstile:Secret"];
    if (ServicioCaptcha.EstaActivo(site, secreto))
        return (site!, secreto!);
    if (!env.IsDevelopment())
        return null;
    return ("1x00000000000000000000AA", "1x0000000000000000000000000000000AA");
}

static async Task<bool> CaptchaValido(IHttpClientFactory clientes, string secreto, string token, string? ip)
{
    var cuerpo = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["secret"] = secreto,
        ["response"] = token,
        ["remoteip"] = ip ?? ""
    });
    using var resp = await clientes.CreateClient("turnstile").PostAsync("turnstile/v0/siteverify", cuerpo);
    if (!resp.IsSuccessStatusCode) return false;
    using var json = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
    return json.RootElement.TryGetProperty("success", out var exito) && exito.ValueKind == JsonValueKind.True;
}

static string AperturaContacto(IDataProtectionProvider proteccion) =>
    proteccion.CreateProtector("contacto-apertura")
        .Protect(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

static TimeSpan? EdadApertura(IDataProtectionProvider proteccion, string? apertura)
{
    if (string.IsNullOrWhiteSpace(apertura) || apertura.Length > 400)
        return null;
    try
    {
        var texto = proteccion.CreateProtector("contacto-apertura").Unprotect(apertura);
        if (!long.TryParse(texto, out var unix))
            return null;
        return DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unix);
    }
    catch (System.Security.Cryptography.CryptographicException)
    {
        return null;
    }
}

static async Task<bool> FormularioContactoActivo(AppDbContext db)
{
    var valor = await db.Ajustes.AsNoTracking()
        .Where(a => a.Clave == Ajuste.FormularioContacto)
        .Select(a => a.Valor)
        .FirstOrDefaultAsync();
    return ServicioContacto.EstaActivo(valor);
}

static async Task PurgarMensajesLeidos(AppDbContext db)
{
    var ahora = DateTimeOffset.UtcNow;
    var limite = ahora.AddDays(-ServicioContacto.DiasHastaBorrar);
    var vencidos = await db.MensajesContacto.Where(m => m.Leido != null && m.Leido <= limite).ToListAsync();
    if (vencidos.Count == 0) return;
    db.MensajesContacto.RemoveRange(vencidos);
    await db.SaveChangesAsync();
}

static async Task<string> EmailVentas(AppDbContext db) =>
    await db.Ajustes.AsNoTracking()
        .Where(a => a.Clave == Ajuste.EmailVentas)
        .Select(a => a.Valor)
        .FirstOrDefaultAsync() ?? ServicioAdmin.EmailVentasGenerico;

static async Task AsegurarEmailVentas(AppDbContext db)
{
    if (await db.Ajustes.AnyAsync(a => a.Clave == Ajuste.EmailVentas)) return;
    db.Ajustes.Add(new Ajuste { Clave = Ajuste.EmailVentas, Valor = ServicioAdmin.EmailVentasGenerico });
    await db.SaveChangesAsync();
}

static async Task GuardarAjuste(AppDbContext db, string clave, string valor)
{
    var ajuste = await db.Ajustes.FirstOrDefaultAsync(a => a.Clave == clave);
    if (ajuste == null)
        db.Ajustes.Add(new Ajuste { Clave = clave, Valor = valor });
    else
        ajuste.Valor = valor;
}

static async Task AsegurarTelefonoVentas(AppDbContext db, string? configurado)
{
    if (await db.Ajustes.AnyAsync(a => a.Clave == Ajuste.TelefonoVentas)) return;
    if (string.IsNullOrWhiteSpace(configurado)) return;
    try
    {
        db.Ajustes.Add(new Ajuste { Clave = Ajuste.TelefonoVentas, Valor = ServicioAdmin.GuardarTelefonoVentas(configurado) });
        await db.SaveChangesAsync();
    }
    catch (ErrorNegocio)
    {
    }
}

static async Task<IResult> CambiarActivoLocal(int idLocal, bool activo, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config)
{
    if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
    var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == idLocal);
    if (local == null) return Results.NotFound();
    try
    {
        ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
        if (activo) ServicioAdmin.Activar(local);
        else ServicioAdmin.Desactivar(local);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}

static string? UrlContacto(string telefono, string texto = "Hola")
{
    if (string.IsNullOrWhiteSpace(telefono)) return null;
    try
    {
        return ServicioOrdenCompra.UrlWhatsApp(telefono, texto);
    }
    catch (ErrorNegocio)
    {
        return null;
    }
}

static object OrdenDto(OrdenCompra orden) => new
{
    orden.Id,
    orden.IdProveedor,
    orden.Fecha,
    orden.NombreUsuario,
    orden.Recibida,
    orden.FechaRecepcion,
    orden.NombreUsuarioRecepcion,
    lineas = orden.Lineas.OrderBy(l => l.Id)
        .Select(l => new { l.Id, l.IdProducto, l.Codigo, l.Nombre, l.Cantidad, l.CantidadRecibida })
};

static string? ValidarProveedorPedido(ProveedorPedido pedido)
{
    var nombre = pedido.Nombre?.Trim() ?? "";
    if (nombre.Length == 0) return "El nombre es obligatorio.";
    if (nombre.Length > 200 || (pedido.TipoProductos?.Length ?? 0) > 200) return "Nombre o tipo de productos demasiado largo.";
    pedido.TipoProductos ??= "";
    pedido.Telefono = pedido.Telefono?.Trim() ?? "";
    if (pedido.Telefono.Length == 0) return null;
    if (pedido.Telefono.Length > 30) return "El teléfono es demasiado largo.";
    try
    {
        ServicioOrdenCompra.NormalizarTelefono(pedido.Telefono);
        return null;
    }
    catch (ErrorNegocio ex)
    {
        return ex.Message;
    }
}

static void GuardarConfiguracion(AppDbContext db, int idLocal, string clave, object valor)
{
    var fila = db.Configuracion.Local.FirstOrDefault(c => c.IdLocal == idLocal && c.Clave == clave);
    if (fila == null)
    {
        fila = new Configuracion { IdLocal = idLocal, Clave = clave };
        db.Configuracion.Add(fila);
    }
    fila.Valor = valor switch
    {
        bool b => b ? "1" : "0",
        decimal d => d.ToString(CultureInfo.InvariantCulture),
        _ => Convert.ToString(valor, CultureInfo.InvariantCulture) ?? ""
    };
}

static decimal FactorLibre(List<Configuracion> filas, int idLocal, string clave)
{
    var valor = filas.FirstOrDefault(c => c.IdLocal == idLocal && c.Clave == clave)?.Valor;
    return decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var factor) && factor > 0
        ? factor
        : 1m;
}

static decimal DecimalConfig(string valor, decimal defecto) =>
    decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultado) ? resultado : defecto;

static bool BoolConfig(string valor) => valor is "1" or "true" or "True";

static int IntConfig(string valor, int defecto) =>
    int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var resultado) && resultado >= 1 ? resultado : defecto;

static async Task<bool> EsperaMercadoPago(AppDbContext db, VentaPedido pedido)
{
    var ids = pedido.Pagos.Select(p => p.IdMetodoPago).Distinct().ToList();
    if (ids.Count == 0 && !string.IsNullOrWhiteSpace(pedido.MetodoPago))
    {
        var unico = await db.MetodosPago.FirstOrDefaultAsync(m => m.Activo && m.Descripcion == pedido.MetodoPago);
        if (unico != null) ids.Add(unico.Id);
    }
    if (ids.Count == 0) return false;
    var integraciones = await db.MetodosPago.Where(m => ids.Contains(m.Id)).Select(m => m.Integracion).ToListAsync();
    return integraciones.Any(IntegracionPago.EsMercadoPago);
}

static string RedirectMercadoPago(IConfiguration config)
{
    var propia = config["MercadoPago:RedirectUri"];
    if (!string.IsNullOrWhiteSpace(propia)) return propia;
    var api = config["App:UrlApi"];
    if (string.IsNullOrWhiteSpace(api))
        throw new InvalidOperationException("Falta MercadoPago:RedirectUri.");
    return $"{api.TrimEnd('/')}/mercadopago/vuelta";
}

static object VistaCobro(CobroMercadoPago cobro) => new
{
    cobro.Id,
    cobro.Estado,
    cobro.Modo,
    cobro.Importe,
    qrData = cobro.QrData,
    cobro.IdInformeVenta
};

static async Task<List<PagoSolicitado>> MetodoUnico(AppDbContext db, string descripcion, decimal total)
{
    if (string.IsNullOrWhiteSpace(descripcion))
        throw new ErrorNegocio("Elegí un medio de pago.");
    var medio = await db.MetodosPago.FirstOrDefaultAsync(m => m.Activo && m.Descripcion == descripcion);
    if (medio == null)
        throw new ErrorNegocio("El medio de pago no existe o está inactivo.");
    return [new PagoSolicitado(medio.Id, total)];
}

static async Task<List<int>> LocalesAccesibles(AppDbContext db, SesionActual sesion)
{
    if (sesion.IdUsuario == null) return [];
    return await db.UsuarioLocales.Where(x => x.IdUsuario == sesion.IdUsuario).Select(x => x.IdLocal).ToListAsync();
}

static async Task<List<int>> LocalesDeLaCuenta(AppDbContext db, SesionActual sesion)
{
    if (sesion.IdUsuario == null) return [];
    var vinculos = await db.UsuarioLocales.AsNoTracking()
        .Select(x => new { x.IdUsuario, x.IdLocal })
        .ToListAsync();
    var usuarios = await db.Usuarios.AsNoTracking()
        .Select(u => new { u.Id, u.Rol })
        .ToListAsync();
    var ids = ServicioUsuarios.LocalesDeLaCuenta(
        sesion.IdUsuario.Value,
        sesion.Rol ?? "",
        vinculos.Select(v => (v.IdUsuario, v.IdLocal)),
        usuarios.Select(u => (u.Id, u.Rol))).ToList();
    return await db.Locales.AsNoTracking()
        .Where(l => ids.Contains(l.IdLocal) && l.Activo)
        .Select(l => l.IdLocal)
        .ToListAsync();
}

static async Task<bool> EstaEnLaCuenta(AppDbContext db, int idUsuario, IReadOnlyCollection<int> locales) =>
    await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == idUsuario && locales.Contains(x.IdLocal));

static async Task<IResult> CambiarEstadoUsuario(int id, bool activo, AppDbContext db, SesionActual sesion)
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.IdUsuario == id) return Results.BadRequest("No podés desactivar tu propio usuario.");
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    var locales = await LocalesDeLaCuenta(db, sesion);
    if (usuario == null || !await EstaEnLaCuenta(db, id, locales)) return Results.NotFound();
    if (!PuedeEditarUsuario(sesion.Rol, usuario.Rol, usuario.Rol)) return Results.Forbid();
    usuario.Activo = activo;
    await db.SaveChangesAsync();
    return Results.Ok(new { usuario.Id, usuario.Activo });
}

static bool PuedeCrearUsuario(string? rol, string rolObjetivo) =>
    Permisos.PuedeAdministrarUsuarios(rol ?? "") &&
    Permisos.EsRolGestionable(rolObjetivo) &&
    (rolObjetivo != Roles.Socio || Permisos.PuedeAsignarSocio(rol ?? ""));

static bool PuedeEditarUsuario(string? rol, string rolActual, string rolNuevo) =>
    Permisos.PuedeAdministrarUsuarios(rol ?? "") &&
    Permisos.EsRolGestionable(rolActual) &&
    Permisos.EsRolGestionable(rolNuevo) &&
    (rolNuevo != Roles.Socio || Permisos.PuedeAsignarSocio(rol ?? "")) &&
    (rol != Roles.Socio || rolActual == Roles.Empleado);

static string? ValidarUsuarioPedido(UsuarioPedido pedido, bool requiereClave)
{
    if (string.IsNullOrWhiteSpace(pedido.Nombre) || string.IsNullOrWhiteSpace(pedido.NombreUsuario))
        return "Nombre y usuario son obligatorios.";
    if (!Permisos.EsRolGestionable(pedido.Rol))
        return "El rol no es válido.";
    if (pedido.IdsLocal.Distinct().Count() == 0)
        return "Asigná al menos un local.";
    if (requiereClave && string.IsNullOrWhiteSpace(pedido.Clave))
        return "La clave es obligatoria.";
    if (!string.IsNullOrWhiteSpace(pedido.Clave) && pedido.Clave.Length < 4)
        return "La clave debe tener al menos 4 caracteres.";
    return null;
}

static void CargarEnvLocal()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        var archivo = Path.Combine(dir.FullName, ".env");
        if (File.Exists(archivo))
        {
            foreach (var linea in File.ReadAllLines(archivo))
            {
                var t = linea.Trim();
                if (t.Length == 0 || t.StartsWith('#') || !t.Contains('=')) continue;
                var i = t.IndexOf('=');
                var clave = t[..i].Trim();
                var valor = t[(i + 1)..].Trim();
                if (!string.IsNullOrEmpty(clave) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(clave)))
                    Environment.SetEnvironmentVariable(clave, valor);
            }
            return;
        }
        dir = dir.Parent;
    }
}

