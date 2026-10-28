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
    public DbSet<Categoria> Categorias => Set<Categoria>();
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
    public DbSet<CuentaMercadoPago> CuentasMercadoPago => Set<CuentaMercadoPago>();
    public DbSet<PuestoMercadoPago> PuestosMercadoPago => Set<PuestoMercadoPago>();
    public DbSet<CobroMercadoPago> CobrosMercadoPago => Set<CobroMercadoPago>();
    public DbSet<PedidoEspera> PedidosEspera => Set<PedidoEspera>();
    public DbSet<LineaPedidoEspera> LineasPedidoEspera => Set<LineaPedidoEspera>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<ProductoProveedor> ProductosProveedor => Set<ProductoProveedor>();
    public DbSet<OfertaCombo> OfertasCombo => Set<OfertaCombo>();
    public DbSet<LineaOfertaCombo> LineasOfertaCombo => Set<LineaOfertaCombo>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CuentaPendiente> CuentasPendientes => Set<CuentaPendiente>();
    public DbSet<LineaCuenta> LineasCuenta => Set<LineaCuenta>();
    public DbSet<PagoCuenta> PagosCuenta => Set<PagoCuenta>();
    public DbSet<ImagenProducto> ImagenesProducto => Set<ImagenProducto>();
    public DbSet<ImagenRechazada> ImagenesRechazadas => Set<ImagenRechazada>();
    public DbSet<PedidoTienda> PedidosTienda => Set<PedidoTienda>();
    public DbSet<LineaPedidoTienda> LineasPedidoTienda => Set<LineaPedidoTienda>();

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
            e.Property(x => x.AliasTienda).HasMaxLength(ServicioTienda.LargoMaximoAlias);
            e.HasIndex(x => x.AliasTienda).IsUnique().HasFilter("\"AliasTienda\" IS NOT NULL");
            e.Property(x => x.EstadoTienda).HasMaxLength(20).HasDefaultValue(ServicioTienda.Inactiva);
            e.Property(x => x.Direccion).HasMaxLength(ServicioTienda.LargoMaximoDato).HasDefaultValue("");
            e.Property(x => x.Horario).HasMaxLength(ServicioTienda.LargoMaximoDato).HasDefaultValue("");
            e.Property(x => x.WhatsApp).HasMaxLength(20).HasDefaultValue("");
            e.Property(x => x.Latitud).HasPrecision(9, 6);
            e.Property(x => x.Longitud).HasPrecision(9, 6);
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
            e.Property(x => x.EmailConfirmado).HasDefaultValue(true);
            e.Property(x => x.TokenConfirmacion).HasMaxLength(64);
            e.HasIndex(x => x.TokenConfirmacion).IsUnique().HasFilter("\"TokenConfirmacion\" IS NOT NULL");
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
            e.HasIndex(x => new { x.IdLocal, x.Publicado });
            e.HasOne<Categoria>().WithMany().HasForeignKey(x => x.IdCategoria).OnDelete(DeleteBehavior.SetNull);
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
            e.HasOne<Categoria>().WithMany().HasForeignKey(x => x.IdCategoria).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(ServicioCategorias.LargoMaximoNombre);
            e.HasIndex(x => x.IdLocal);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<MetodoPago>(e =>
        {
            e.ToTable("MetodosPago");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Descripcion).HasMaxLength(80);
            e.Property(x => x.Integracion).HasMaxLength(20);
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
            e.Property(x => x.DescripcionOferta).HasMaxLength(20).HasDefaultValue("");
            ConfigurarAhorros(e);
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
            e.Property(x => x.NombrePack).HasMaxLength(30).HasDefaultValue("");
            e.Property(x => x.UnidadesPack).HasDefaultValue(1);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<ProductoProveedor>(e =>
        {
            e.ToTable("ProductosProveedor");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.NombrePack).HasMaxLength(30);
            e.HasIndex(x => new { x.IdLocal, x.IdProveedor, x.IdProducto }).IsUnique();
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

        model.Entity<CuentaMercadoPago>(e =>
        {
            e.ToTable("CuentasMercadoPago");
            e.HasKey(x => x.IdLocal);
            e.Property(x => x.TokenAcceso).HasColumnType("text");
            e.Property(x => x.TokenRenovacion).HasColumnType("text");
            e.Property(x => x.IdUsuarioMp).HasMaxLength(32);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<PuestoMercadoPago>(e =>
        {
            e.ToTable("PuestosMercadoPago");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.IdExterno).HasMaxLength(60);
            e.Property(x => x.IdSucursal).HasMaxLength(32);
            e.Property(x => x.UrlQr).HasMaxLength(500);
            e.Property(x => x.IdLector).HasMaxLength(80);
            e.HasIndex(x => new { x.IdLocal, x.NroCaja }).IsUnique();
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<CobroMercadoPago>(e =>
        {
            e.ToTable("CobrosMercadoPago");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Modo).HasMaxLength(20);
            e.Property(x => x.Estado).HasMaxLength(20);
            e.Property(x => x.Importe).HasPrecision(14, 2);
            e.Property(x => x.IdOrdenMp).HasMaxLength(64);
            e.Property(x => x.QrData).HasColumnType("text");
            e.Property(x => x.PedidoJson).HasColumnType("text");
            e.HasIndex(x => x.IdOrdenMp).IsUnique().HasFilter("\"IdOrdenMp\" <> ''");
            e.HasIndex(x => new { x.IdLocal, x.NroCaja })
                .IsUnique()
                .HasFilter("\"Estado\" = 'esperando' AND \"Modo\" = 'qr_impreso'")
                .HasDatabaseName("IX_CobrosMercadoPago_QrAbierto");
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<PedidoEspera>(e =>
        {
            e.ToTable("PedidosEspera");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Nota).HasMaxLength(80);
            e.HasIndex(x => new { x.IdLocal, x.NroCaja, x.Fecha });
            e.HasMany(x => x.Lineas).WithOne().HasForeignKey(l => l.IdPedidoEspera).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<LineaPedidoEspera>(e =>
        {
            e.ToTable("LineasPedidoEspera");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Codigo).HasMaxLength(80);
            e.Property(x => x.Nombre).HasMaxLength(200);
            e.Property(x => x.Cantidad).HasPrecision(14, 3);
            e.Property(x => x.PrecioUnitario).HasPrecision(14, 2);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Oferta>(e =>
        {
            e.ToTable("Ofertas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Tipo).HasMaxLength(20);
            e.Property(x => x.PrecioUnidad).HasPrecision(14, 2);
            e.Ignore(x => x.SinFin);
            e.Property(x => x.AnulaConAlCosto).HasDefaultValue(true).HasSentinel(true);
            e.HasIndex(x => new { x.IdLocal, x.IdProducto, x.FechaDesde });
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<OfertaCombo>(e =>
        {
            e.ToTable("OfertasCombo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(60);
            e.Property(x => x.PrecioTotal).HasPrecision(14, 2);
            e.HasIndex(x => new { x.IdLocal, x.FechaDesde });
            e.HasMany(x => x.Lineas).WithOne().HasForeignKey(l => l.IdOfertaCombo).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<LineaOfertaCombo>(e =>
        {
            e.ToTable("LineasOfertaCombo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Nombre).HasMaxLength(80);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.HasIndex(x => new { x.IdLocal, x.Nombre });
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<CuentaPendiente>(e =>
        {
            e.ToTable("CuentasPendientes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.NombreUsuario).HasMaxLength(80);
            e.Property(x => x.Importe).HasPrecision(14, 2);
            e.Property(x => x.Saldo).HasPrecision(14, 2).IsConcurrencyToken();
            e.Property(x => x.GananciaEstimada).HasPrecision(14, 2);
            e.Property(x => x.Estado).HasMaxLength(20);
            e.Property(x => x.UsuarioIncobrable).HasMaxLength(80);
            e.HasIndex(x => new { x.IdLocal, x.Estado, x.IdCliente });
            e.HasIndex(x => new { x.IdLocal, x.Fecha });
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Lineas).WithOne().HasForeignKey(l => l.IdCuentaPendiente).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Pagos).WithOne().HasForeignKey(p => p.IdCuentaPendiente).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<LineaCuenta>(e =>
        {
            e.ToTable("LineasCuenta");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Codigo).HasMaxLength(80);
            e.Property(x => x.Nombre).HasMaxLength(200);
            e.Property(x => x.Cantidad).HasPrecision(14, 3);
            e.Property(x => x.Precio).HasPrecision(14, 2);
            e.Property(x => x.Costo).HasPrecision(14, 2);
            e.Property(x => x.SubTotal).HasPrecision(14, 2);
            e.Property(x => x.DescripcionOferta).HasMaxLength(20);
            ConfigurarAhorros(e);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<PagoCuenta>(e =>
        {
            e.ToTable("PagosCuenta");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Importe).HasPrecision(14, 2);
            e.HasIndex(x => x.IdInformeVenta);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<ImagenProducto>(e =>
        {
            e.ToTable("ImagenesProducto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Archivo).HasMaxLength(ServicioImagenesTienda.LargoMaximoArchivo);
            e.Property(x => x.TipoContenido).HasMaxLength(20);
            e.Property(x => x.Origen).HasMaxLength(20);
            e.Property(x => x.Fuente).HasMaxLength(60);
            e.Property(x => x.Enlace).HasMaxLength(200);
            e.Property(x => x.Licencia).HasMaxLength(20);
            e.HasIndex(x => new { x.IdLocal, x.IdProducto, x.Orden }).IsUnique();
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<ImagenRechazada>(e =>
        {
            e.ToTable("ImagenesRechazadas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Url).HasMaxLength(500);
            e.HasIndex(x => new { x.IdLocal, x.IdProducto });
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<PedidoTienda>(e =>
        {
            e.ToTable("PedidosTienda");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.HashCodigo).HasMaxLength(64);
            e.Property(x => x.Estado).HasMaxLength(20);
            e.Property(x => x.Nombre).HasMaxLength(ServicioPedidosTienda.LargoMaximoNombre);
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(ServicioPedidosTienda.LargoMaximoEmail);
            e.Property(x => x.Subtotal).HasPrecision(14, 2);
            e.Property(x => x.Total).HasPrecision(14, 2);
            e.HasIndex(x => new { x.IdLocal, x.Numero }).IsUnique();
            e.HasIndex(x => x.HashCodigo).IsUnique();
            e.HasIndex(x => new { x.IdLocal, x.Estado });
            e.HasMany(x => x.Lineas).WithOne().HasForeignKey(l => l.IdPedidoTienda).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });

        model.Entity<LineaPedidoTienda>(e =>
        {
            e.ToTable("LineasPedidoTienda");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Codigo).HasMaxLength(80);
            e.Property(x => x.Nombre).HasMaxLength(200);
            e.Property(x => x.Cantidad).HasPrecision(14, 3);
            e.Property(x => x.PrecioUnitario).HasPrecision(14, 2);
            e.Property(x => x.SubTotal).HasPrecision(14, 2);
            e.HasQueryFilter(x => _sesion.IdLocal != null && x.IdLocal == _sesion.IdLocal);
        });
    }

    private static void ConfigurarAhorros<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> e)
        where T : class, ILineaVendida
    {
        e.Property<decimal?>(nameof(ILineaVendida.PrecioBase)).HasPrecision(14, 2);
        e.Property<decimal>(nameof(ILineaVendida.AhorroOferta)).HasPrecision(14, 2).HasDefaultValue(0m);
        e.Property<decimal>(nameof(ILineaVendida.AhorroDescuento)).HasPrecision(14, 2).HasDefaultValue(0m);
        e.Property<decimal>(nameof(ILineaVendida.AhorroCombo)).HasPrecision(14, 2).HasDefaultValue(0m);
        e.Property<string>(nameof(ILineaVendida.NombreCombo)).HasMaxLength(60).HasDefaultValue("");
    }
}
