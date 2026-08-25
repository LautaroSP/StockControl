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
    public DbSet<InformeVenta> InformeVenta => Set<InformeVenta>();
    public DbSet<InformeVentaDetalle> InformeVentaDetalle => Set<InformeVentaDetalle>();
    public DbSet<Configuracion> Configuracion => Set<Configuracion>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Local>(e =>
        {
            e.ToTable("Locales");
            e.HasKey(x => x.IdLocal);
            e.Property(x => x.IdLocal).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(200);
            e.Property(x => x.EstadoAbono).HasMaxLength(40);
        });

        model.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.HasIndex(x => x.NombreUsuario).IsUnique();
            e.Property(x => x.NombreUsuario).HasMaxLength(80);
            e.Property(x => x.Rol).HasMaxLength(20);
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
            e.HasMany(x => x.Detalles).WithOne().HasForeignKey(d => d.IdInformeVenta);
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
    }
}
