using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockControl.Api.Auth;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Api.Importacion;
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
}

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

app.MapPost("/auth/login", async (LoginPedido pedido, AppDbContext db, PasswordHasher<Usuario> hasher, JwtServicio jwt) =>
{
    if (string.IsNullOrWhiteSpace(pedido.Usuario) || string.IsNullOrWhiteSpace(pedido.Clave))
        return Results.BadRequest("Usuario y clave son obligatorios.");
    var u = await db.Usuarios.IgnoreQueryFilters()
        .FirstOrDefaultAsync(x => x.NombreUsuario == pedido.Usuario);
    if (u == null || hasher.VerifyHashedPassword(u, u.HashClave, pedido.Clave) == PasswordVerificationResult.Failed)
        return Results.Unauthorized();
    return Results.Ok(new { token = jwt.Emitir(u, null), rol = u.Rol, nombre = u.NombreUsuario });
}).RequireRateLimiting("api");

app.MapGet("/locales", async (AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    var ids = await db.UsuarioLocales.Where(x => x.IdUsuario == sesion.IdUsuario).Select(x => x.IdLocal).ToListAsync();
    var locales = await db.Locales.Where(l => ids.Contains(l.IdLocal)).ToListAsync();
    return Results.Ok(locales.Select(l => new { l.IdLocal, l.Nombre, l.EstadoAbono, l.Vence }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/locales/{id:int}/entrar", async (int id, AppDbContext db, SesionActual sesion, JwtServicio jwt) =>
{
    if (sesion.IdUsuario == null) return Results.Unauthorized();
    var ok = await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == sesion.IdUsuario && x.IdLocal == id);
    if (!ok) return Results.Forbid();
    var u = await db.Usuarios.IgnoreQueryFilters().FirstAsync(x => x.Id == sesion.IdUsuario);
    return Results.Ok(new { token = jwt.Emitir(u, id) });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/productos", async (AppDbContext db, SesionActual sesion, string? q, string? codigo, bool? sector, string? tipo, int pagina = 1, int tamano = 50) =>
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
    if (!string.IsNullOrWhiteSpace(codigo))
        consulta = consulta.Where(p => p.Codigo == codigo);
    else if (!string.IsNullOrWhiteSpace(q))
        consulta = consulta.Where(p => EF.Functions.ILike(p.Nombre, $"%{q}%") || EF.Functions.ILike(p.Codigo, $"%{q}%"));
    consulta = consulta.OrderBy(p => p.Nombre);
    var total = await consulta.CountAsync();
    var items = await consulta.Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
    return Results.Ok(new { total, pagina, tamano, items });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/productos", async (ProductoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeCrearProducto(sesion.Rol ?? "")) return Results.Forbid();
    if (string.IsNullOrWhiteSpace(pedido.Codigo) || string.IsNullOrWhiteSpace(pedido.Nombre))
        return Results.BadRequest("Código y nombre son obligatorios.");
    if (await db.Productos.AnyAsync(p => p.Codigo == pedido.Codigo))
        return Results.Conflict("Ya existe un producto con ese código.");
    var p = new Producto
    {
        IdLocal = sesion.IdLocal.Value,
        Codigo = pedido.Codigo,
        Nombre = pedido.Nombre,
        Cantidad = pedido.Cantidad,
        Costo = pedido.Costo,
        Precio = pedido.Precio,
        ProductoSector = pedido.ProductoSector,
        FechaModificacion = DateTimeOffset.UtcNow
    };
    db.Productos.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/productos/{p.Id}", p);
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
    await db.SaveChangesAsync();
    return Results.Ok(p);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/productos/{id:int}/stock", async (int id, StockPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (pedido.Cantidad <= 0) return Results.BadRequest("La cantidad tiene que ser mayor a 0.");
    var p = await db.Productos.FirstOrDefaultAsync(x => x.Id == id);
    if (p == null) return Results.NotFound();
    p.Cantidad += pedido.Cantidad;
    p.FechaModificacion = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(p);
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/ventas", async (VentaPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (pedido.CobrarAlCosto && !Permisos.PuedeCobrarAlCosto(sesion.Rol ?? ""))
        return Results.Forbid();
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
            DetalleAdjunto = true
        };
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
        db.InformeVenta.Add(venta);
        await db.SaveChangesAsync();
        return Results.Created($"/ventas/{venta.IdInformeVenta}", new { venta.IdInformeVenta, venta.Total });
    }
    catch (ErrorNegocio ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("api");

app.MapGet("/ventas", async (AppDbContext db, SesionActual sesion, int pagina = 1, int tamano = 50) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    tamano = Math.Clamp(tamano, 1, maxPagina);
    pagina = Math.Max(1, pagina);
    var desde = DateTimeOffset.UtcNow.Subtract(DateTimeOffset.UtcNow.TimeOfDay);
    var q = db.InformeVenta.Where(v => v.Fecha >= desde).OrderByDescending(v => v.Fecha);
    var total = await q.CountAsync();
    var items = await q.Skip((pagina - 1) * tamano).Take(tamano)
        .Select(v => new { v.IdInformeVenta, v.Fecha, v.Total, v.MetodoPago, v.Descuento })
        .ToListAsync();
    return Results.Ok(new { total, pagina, tamano, items });
}).RequireAuthorization().RequireRateLimiting("api");

app.Run();

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

