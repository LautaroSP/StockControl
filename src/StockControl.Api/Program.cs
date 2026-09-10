using System.Text;
using System.Threading.RateLimiting;
using System.Globalization;
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
    if (u == null || !u.Activo || hasher.VerifyHashedPassword(u, u.HashClave, pedido.Clave) == PasswordVerificationResult.Failed)
        return Results.Unauthorized();
    return Results.Ok(new { token = jwt.Emitir(u, null), rol = u.Rol, nombre = string.IsNullOrWhiteSpace(u.Nombre) ? u.NombreUsuario : u.Nombre });
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

app.MapGet("/productos", async (AppDbContext db, SesionActual sesion, string? q, string? codigo, bool? sector, string? tipo, int? idGrupo, bool? sinGrupo, bool? stockBajo, int pagina = 1, int tamano = 50) =>
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
    var p = new Producto
    {
        IdLocal = sesion.IdLocal.Value,
        Codigo = pedido.Codigo,
        Nombre = pedido.Nombre,
        Cantidad = pedido.Cantidad,
        Costo = pedido.Costo,
        Precio = pedido.Precio,
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
    var locales = await LocalesAccesibles(db, sesion);
    var idsUsuarios = await db.UsuarioLocales.Where(x => x.IdLocal == sesion.IdLocal)
        .Select(x => x.IdUsuario).Distinct().ToListAsync();
    var usuarios = await db.Usuarios.Where(u => idsUsuarios.Contains(u.Id) && (u.Rol == Roles.Empleado || u.Rol == Roles.Socio))
        .OrderBy(u => u.Nombre).ThenBy(u => u.NombreUsuario).ToListAsync();
    var asignaciones = await db.UsuarioLocales.Where(x => idsUsuarios.Contains(x.IdUsuario) && locales.Contains(x.IdLocal)).ToListAsync();
    var nombresLocales = await db.Locales.Where(l => locales.Contains(l.IdLocal)).ToDictionaryAsync(l => l.IdLocal, l => l.Nombre);
    return Results.Ok(usuarios.Select(u => new
    {
        u.Id,
        nombre = string.IsNullOrWhiteSpace(u.Nombre) ? u.NombreUsuario : u.Nombre,
        u.NombreUsuario,
        u.Rol,
        u.Activo,
        locales = asignaciones.Where(a => a.IdUsuario == u.Id).Select(a => new { a.IdLocal, nombre = nombresLocales[a.IdLocal] })
    }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/usuarios", async (UsuarioPedido pedido, AppDbContext db, SesionActual sesion, PasswordHasher<Usuario> hasher) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!PuedeCrearUsuario(sesion.Rol, pedido.Rol)) return Results.Forbid();
    var validacion = ValidarUsuarioPedido(pedido, true);
    if (validacion != null) return Results.BadRequest(validacion);
    if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == pedido.NombreUsuario.Trim()))
        return Results.Conflict("Ya existe ese nombre de usuario.");
    var locales = await LocalesAccesibles(db, sesion);
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
    if (!await PerteneceAlLocal(db, id, sesion.IdLocal.Value)) return Results.NotFound();
    if (!PuedeEditarUsuario(sesion.Rol, usuario.Rol, pedido.Rol)) return Results.Forbid();
    var validacion = ValidarUsuarioPedido(pedido, false);
    if (validacion != null) return Results.BadRequest(validacion);
    if (await db.Usuarios.AnyAsync(u => u.Id != id && u.NombreUsuario == pedido.NombreUsuario.Trim()))
        return Results.Conflict("Ya existe ese nombre de usuario.");
    var locales = await LocalesAccesibles(db, sesion);
    var idsLocal = pedido.IdsLocal.Distinct().ToList();
    if (!idsLocal.All(locales.Contains)) return Results.Forbid();
    usuario.Nombre = pedido.Nombre.Trim();
    usuario.NombreUsuario = pedido.NombreUsuario.Trim();
    usuario.Rol = pedido.Rol;
    var actuales = await db.UsuarioLocales.Where(x => x.IdUsuario == id).ToListAsync();
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
    if (usuario == null || !await PerteneceAlLocal(db, id, sesion.IdLocal.Value)) return Results.NotFound();
    if (!PuedeEditarUsuario(sesion.Rol, usuario.Rol, usuario.Rol)) return Results.Forbid();
    if (string.IsNullOrWhiteSpace(pedido.Clave) || pedido.Clave.Length < 4) return Results.BadRequest("La clave debe tener al menos 4 caracteres.");
    usuario.HashClave = hasher.HashPassword(usuario, pedido.Clave);
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
    return Results.Ok(medios.Select(m => new { m.Id, m.Descripcion, m.Activo }));
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/medios-pago", async (MetodoPagoPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (!Permisos.PuedeAdministrarMetodosPago(sesion.Rol ?? "")) return Results.Forbid();
    var descripcion = pedido.Descripcion.Trim();
    if (descripcion.Length == 0) return Results.BadRequest("La descripción es obligatoria.");
    if (await db.MetodosPago.AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower()))
        return Results.Conflict("Ya existe ese medio de pago.");
    var medio = new MetodoPago { IdLocal = sesion.IdLocal.Value, Descripcion = descripcion };
    db.MetodosPago.Add(medio);
    await db.SaveChangesAsync();
    return Results.Created($"/medios-pago/{medio.Id}", new { medio.Id, medio.Descripcion, medio.Activo });
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
    medio.Descripcion = descripcion;
    await db.SaveChangesAsync();
    return Results.Ok(new { medio.Id, medio.Descripcion, medio.Activo });
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
    return Results.Ok(new { medio.Id, medio.Descripcion, medio.Activo });
}).RequireAuthorization().RequireRateLimiting("api");

app.MapPost("/ventas", async (VentaPedido pedido, AppDbContext db, SesionActual sesion) =>
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.NroCaja == null) return Results.BadRequest("Elegí una caja.");
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
        Iva = DecimalConfig(valores["IVA"], 1.21m),
        StockRigido = BoolConfig(valores["StockRigido"]),
        UmbralStockBajo = DecimalConfig(valores["UmbralStockBajo"], 5),
        EmpleadoPuedeModificarPrecios = BoolConfig(valores["EmpleadoPuedeModificarPrecios"]),
        FormatoTicket = ServicioTicket.FormatoODefault(valores["FormatoTicket"]),
        ImprimirTicketAlCobrar = BoolConfig(valores["ImprimirTicketAlCobrar"]),
        CantidadCajas = IntConfig(valores["CantidadCajas"], 1)
    };
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

static decimal DecimalConfig(string valor, decimal defecto) =>
    decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultado) ? resultado : defecto;

static bool BoolConfig(string valor) => valor is "1" or "true" or "True";

static int IntConfig(string valor, int defecto) =>
    int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var resultado) && resultado >= 1 ? resultado : defecto;

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

static async Task<bool> PerteneceAlLocal(AppDbContext db, int idUsuario, int idLocal) =>
    await db.UsuarioLocales.AnyAsync(x => x.IdUsuario == idUsuario && x.IdLocal == idLocal);

static async Task<IResult> CambiarEstadoUsuario(int id, bool activo, AppDbContext db, SesionActual sesion)
{
    if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
    if (sesion.IdUsuario == id) return Results.BadRequest("No podés desactivar tu propio usuario.");
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    if (usuario == null || !await PerteneceAlLocal(db, id, sesion.IdLocal.Value)) return Results.NotFound();
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

