using Microsoft.EntityFrameworkCore;
using StockControl.Dominio;

namespace StockControl.Api.Datos;

public class AppDbContext : DbContext
{
    private readonly SesionActual _sesion;

    public AppDbContext(DbContextOptions<AppDbContext> options, SesionActual sesion)
        : base(options)
    {
        _sesion = sesion;
    }

    public DbSet<Local> Locales => Set<Local>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioLocal> UsuarioLocales => Set<UsuarioLocal>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<GrupoProductos> GrupoProductos => Set<GrupoProductos>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<PagoVenta> PagosVenta => Set<PagoVenta>();
    public DbSet<InformeVenta> InformeVenta => Set<InformeVenta>();
    public DbSet<InformeVentaDetalle> InformeVentaDetalle => Set<InformeVentaDetalle>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<OcupacionCaja> OcupacionesCaja => Set<OcupacionCaja>();
    public DbSet<Configuracion> Configuracion => Set<Configuracion>();
    public DbSet<PlantillaCartel> PlantillasCartel => Set<PlantillaCartel>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraLinea> OrdenesCompraLineas => Set<OrdenCompraLinea>();
    public DbSet<Abono> Abonos => Set<Abono>();
    public DbSet<Ajuste> Ajustes => Set<Ajuste>();
    public DbSet<MensajeContacto> MensajesContacto => Set<MensajeContacto>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Local>(e =>
        {
            e.ToTable("Locales");
            e.HasKey(x => x.IdLocal);
            e.Property(x => x.IdLocal).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(200);
            e.Property(x => x.Activo).HasDefaultValue(true);
            e.Property(x => x.EstadoAbono).HasMaxLength(40);
            e.Property(x => x.IdSuscripcionMp).HasMaxLength(64);
            e.HasIndex(x => x.IdSuscripcionMp).IsUnique().HasFilter("\"IdSuscripcionMp\" IS NOT NULL");
            e.Property(x => x.EstadoSuscripcion).HasMaxLength(20).HasDefaultValue("");
        });

        model.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.HasIndex(x => x.NombreUsuario).IsUnique();
            e.Property(x => x.NombreUsuario).HasMaxLength(80);
            e.Property(x => x.Email).HasMaxLength(200).HasDefaultValue("");
            e.HasIndex(x => x.Email).IsUnique().HasFilter("\"Email\" <> ''");
            e.Property(x => x.Rol).HasMaxLength(20);
            e.Property(x => x.Activo).HasDefaultValue(true);
        });

        model.Entity<UsuarioLocal>(e =>
        {
            e.ToTable("UsuarioLocales");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.HasIndex(x => new { x.IdUsuario, x.IdLocal }).IsUnique();
        });

        model.Entity<Producto>(e =>
        {
            e.ToTable("Productos");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.IdLocal, x.Codigo }).IsUnique();
            e.Property(x => x.Codigo).HasMaxLength(80);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Cantidad).HasPrecision(14, 3);
            e.Property(x => x.Costo).HasPrecision(14, 2);
            e.Property(x => x.Precio).HasPrecision(14, 2);
            e.Property(x => x.ValorGanancia).HasPrecision(14, 2);
            e.Property(x => x.UsuarioModificacion).HasMaxLength(80);
            e.Ignore(x => x.EsGenerico);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<GrupoProductos>(e =>
        {
            e.ToTable("GrupoProductos");
            e.HasKey(x => x.IdGrupoProducto);
            e.Property(x => x.IdGrupoProducto).UseIdentityByDefaultColumn();
            e.Property(x => x.PrecioGrupo).HasPrecision(14, 2);
            e.Property(x => x.Costo).HasPrecision(14, 2);
            e.Property(x => x.Ganancia).HasPrecision(14, 4);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<MetodoPago>(e =>
        {
            e.ToTable("MetodosPago");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Descripcion).HasMaxLength(80);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<PagoVenta>(e =>
        {
            e.ToTable("PagosVenta");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.DescripcionMetodoPago).HasMaxLength(80);
            e.Property(x => x.Importe).HasPrecision(14, 2);
            e.HasIndex(x => new { x.IdLocal, x.IdInformeVenta });
            e.HasOne<InformeVenta>().WithMany(x => x.Pagos)
                .HasForeignKey(x => x.IdInformeVenta)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<InformeVenta>(e =>
        {
            e.ToTable("InformeVenta");
            e.HasKey(x => x.IdInformeVenta);
            e.Property(x => x.IdInformeVenta).UseIdentityByDefaultColumn();
            e.Property(x => x.Total).HasPrecision(14, 2);
            e.Property(x => x.Subtotal).HasPrecision(14, 2);
            e.Property(x => x.Descuento).HasPrecision(14, 2);
            e.HasIndex(x => new { x.IdLocal, x.Fecha });
            e.HasMany(x => x.Detalles).WithOne().HasForeignKey(d => d.IdInformeVenta);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Caja>(e =>
        {
            e.ToTable("Cajas");
            e.HasKey(x => x.IdCaja);
            e.Property(x => x.IdCaja).UseIdentityByDefaultColumn();
            e.Property(x => x.Total).HasPrecision(14, 2);
            e.Property(x => x.MetodoPago).HasMaxLength(80);
            e.Property(x => x.NombreCierre).HasMaxLength(80);
            e.Property(x => x.TipoDesglose).HasMaxLength(20);
            e.HasIndex(x => new { x.IdLocal, x.NroCaja });
            e.HasIndex(x => new { x.IdLocal, x.IdCierre });
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<OcupacionCaja>(e =>
        {
            e.ToTable("OcupacionesCaja");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.NombreUsuario).HasMaxLength(80);
            e.HasIndex(x => new { x.IdLocal, x.NroCaja, x.IdUsuario }).IsUnique();
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<InformeVentaDetalle>(e =>
        {
            e.ToTable("InformeVentaDetalle");
            e.HasKey(x => x.IdInformeVentaDetalle);
            e.Property(x => x.IdInformeVentaDetalle).UseIdentityByDefaultColumn();
            e.Property(x => x.Precio).HasPrecision(14, 2);
            e.Property(x => x.Costo).HasPrecision(14, 2);
            e.Property(x => x.SubTotal).HasPrecision(14, 2);
            e.Property(x => x.Cantidad).HasPrecision(14, 3);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Configuracion>(e =>
        {
            e.ToTable("Configuracion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.IdLocal, x.Clave }).IsUnique();
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<PlantillaCartel>(e =>
        {
            e.ToTable("PlantillasCartel");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Tamano).HasMaxLength(10);
            e.Property(x => x.TipoContenido).HasMaxLength(40);
            e.HasIndex(x => new { x.IdLocal, x.Tamano }).IsUnique();
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedores");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(200);
            e.Property(x => x.TipoProductos).HasMaxLength(200);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<OrdenCompra>(e =>
        {
            e.ToTable("OrdenesCompra");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.NombreUsuario).HasMaxLength(80);
            e.Property(x => x.NombreUsuarioRecepcion).HasMaxLength(80);
            e.Property(x => x.FechaRecepcion).IsConcurrencyToken();
            e.Ignore(x => x.Recibida);
            e.HasIndex(x => new { x.IdLocal, x.IdProveedor, x.Fecha });
            e.HasMany(x => x.Lineas).WithOne().HasForeignKey(l => l.IdOrdenCompra);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<OrdenCompraLinea>(e =>
        {
            e.ToTable("OrdenesCompraLineas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Codigo).HasMaxLength(80);
            e.Property(x => x.Nombre).HasMaxLength(200);
            e.Property(x => x.Cantidad).HasPrecision(14, 3);
            e.Property(x => x.CantidadRecibida).HasPrecision(14, 3);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Abono>(e =>
        {
            e.ToTable("Abonos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Importe).HasPrecision(14, 2);
            e.Property(x => x.Estado).HasMaxLength(20);
            e.Property(x => x.EstadoAnterior).HasMaxLength(40).HasDefaultValue("");
            e.Property(x => x.Origen).HasMaxLength(20);
            e.Property(x => x.Canal).HasMaxLength(20).HasDefaultValue("");
            e.Property(x => x.IdPreferenciaMp).HasMaxLength(100);
            e.Property(x => x.UrlPago).HasMaxLength(500);
            e.Property(x => x.IdPagoMp).HasMaxLength(64);
            e.HasIndex(x => x.IdPagoMp).IsUnique().HasFilter("\"IdPagoMp\" IS NOT NULL");
            e.HasIndex(x => new { x.IdLocal, x.Estado, x.FechaCreacion });
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Ajuste>(e =>
        {
            e.ToTable("Ajustes");
            e.HasKey(x => x.Clave);
            e.Property(x => x.Clave).HasMaxLength(40);
            e.Property(x => x.Valor).HasMaxLength(200);
        });

        model.Entity<MensajeContacto>(e =>
        {
            e.ToTable("MensajesContacto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.Property(x => x.Correo).HasMaxLength(200);
            e.Property(x => x.Mensaje).HasMaxLength(2000);
            e.HasIndex(x => x.Leido);
        });
    }
}
