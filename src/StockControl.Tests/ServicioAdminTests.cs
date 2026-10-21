using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioAdminTests
{
    [Fact]
    public void Solo_el_admin_entra_al_panel()
    {
        Assert.True(ServicioAdmin.PuedeEntrar(Roles.Admin));
        Assert.False(ServicioAdmin.PuedeEntrar(Roles.Dueno));
        Assert.False(ServicioAdmin.PuedeEntrar(Roles.Empleado));
    }

    [Fact]
    public void La_clave_secreta_correcta_pasa()
    {
        var error = Record.Exception(() => ServicioAdmin.ValidarClaveSecreta("clave-local", "clave-local"));

        Assert.Null(error);
    }

    [Fact]
    public void La_clave_secreta_incorrecta_no_pasa()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarClaveSecreta("otra", "clave-local"));

        Assert.Equal("La clave secreta no coincide.", error.Message);
    }

    [Fact]
    public void Sin_clave_configurada_no_se_puede_dar_admin()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarClaveSecreta("clave-local", ""));

        Assert.Equal("No hay clave secreta configurada.", error.Message);
    }

    [Fact]
    public void El_local_nuevo_queda_en_prueba_siete_dias()
    {
        var ahora = new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero);

        var local = ServicioAdmin.NuevoLocal("Sucursal Centro", ahora);

        Assert.Equal("Sucursal Centro", local.Nombre);
        Assert.Equal(ServicioAbono.Trial, local.EstadoAbono);
        Assert.Equal(ahora.AddDays(7), local.Vence);
    }

    [Fact]
    public void El_primer_local_del_dueno_es_el_unico_trial()
    {
        var ahora = new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero);

        var primero = ServicioAdmin.AltaLocal("Uno", ahora, yaTieneLocal: false);
        var segundo = ServicioAdmin.AltaLocal("Dos", ahora, yaTieneLocal: true);
        var estado = ServicioAbono.Estado(segundo, ahora);

        Assert.Equal(ServicioAbono.Trial, primero.EstadoAbono);
        Assert.Equal(ahora.AddDays(7), primero.Vence);
        Assert.Equal(ServicioAbono.Pendiente, segundo.EstadoAbono);
        Assert.Null(segundo.Vence);
        Assert.False(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Copiar_catalogo_repite_el_stock_si_se_pide()
    {
        var ahora = new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero);
        var grupo = new GrupoProductos { IdGrupoProducto = 3, IdLocal = 1, NombreGrupo = "Gaseosas", PrecioGrupo = 2500 };
        var proveedor = new Proveedor { Id = 8, IdLocal = 1, Nombre = "Coca", Telefono = "111", Activo = true };
        Producto[] productos =
        [
            new() { Id = 1, IdLocal = 1, Codigo = "779", Nombre = "Coca 500", Cantidad = 12, Costo = 1000, Precio = 1996.50m, IdGrupoProducto = 3, IdProveedor = 8 },
            new() { Id = 2, IdLocal = 1, Codigo = "GENERIC-1", Nombre = "Varios", Cantidad = 1, Precio = 100 }
        ];

        var copia = ServicioAdmin.CopiarCatalogo([grupo], [proveedor], productos, 9, copiarStock: true, ahora, "admin");

        Assert.Single(copia.Productos);
        Assert.Equal(12, copia.Productos[0].Producto.Cantidad);
        Assert.Equal(1996.50m, copia.Productos[0].Producto.Precio);
        Assert.Equal(9, copia.Productos[0].Producto.IdLocal);
        Assert.Equal("Gaseosas", copia.GrupoPorIdOrigen[3].NombreGrupo);
        Assert.Equal("Coca", copia.ProveedorPorIdOrigen[8].Nombre);
        Assert.Equal(3, copia.Productos[0].IdGrupoOrigen);
    }

    [Fact]
    public void Copiar_catalogo_sin_stock_deja_la_cantidad_en_cero()
    {
        var producto = new Producto { IdLocal = 1, Codigo = "779", Nombre = "Agua", Cantidad = 40, Precio = 1600 };

        var copia = ServicioAdmin.CopiarCatalogo([], [], [producto], 2, copiarStock: false, DateTimeOffset.UtcNow, "admin");

        Assert.Equal(0, copia.Productos[0].Producto.Cantidad);
        Assert.Equal("Agua", copia.Productos[0].Producto.Nombre);
    }

    [Fact]
    public void El_local_sin_nombre_no_se_crea()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioAdmin.NuevoLocal("  ", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void El_local_solo_se_asocia_a_un_dueno()
    {
        ServicioAdmin.ValidarDueno(Roles.Dueno);
        Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarDueno(Roles.Empleado));
    }

    [Fact]
    public void No_se_puede_bloquear_el_propio_usuario()
    {
        var usuario = new Usuario { Id = 4, Activo = true };

        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.Bloquear(usuario, 4));

        Assert.Equal("No podés bloquear tu propio usuario.", error.Message);
        Assert.True(usuario.Activo);
    }

    [Fact]
    public void Bloquear_deja_al_usuario_inactivo()
    {
        var usuario = new Usuario { Id = 4, Activo = true };

        ServicioAdmin.Bloquear(usuario, 1);

        Assert.False(usuario.Activo);
    }

    [Fact]
    public void No_se_puede_eliminar_el_propio_usuario()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarEliminar(3, 3));

        Assert.Equal("No podés eliminar tu propio usuario.", error.Message);
    }

    [Fact]
    public void Tiene_que_quedar_un_administrador()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.AsegurarAdminRestante(Roles.Admin, 0));

        Assert.Equal("Tiene que quedar al menos un administrador.", error.Message);
        ServicioAdmin.AsegurarAdminRestante(Roles.Dueno, 0);
        ServicioAdmin.AsegurarAdminRestante(Roles.Admin, 1);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Dueno)]
    [InlineData(Roles.Empleado)]
    [InlineData(Roles.Socio)]
    public void El_admin_puede_cambiar_la_clave_de_cualquier_cuenta(string rol)
    {
        var clave = ServicioAdmin.ValidarCambioClave(rol, "  nueva-clave  ", "clave-local", "clave-local");

        Assert.Equal("nueva-clave", clave);
    }

    [Fact]
    public void Una_clave_corta_no_se_puede_guardar()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarCambioClave(Roles.Dueno, "abc", "clave-local", "clave-local"));

        Assert.Equal("La clave debe tener al menos 4 caracteres.", error.Message);
    }

    [Fact]
    public void Un_rol_desconocido_no_cambia_la_clave()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarCambioClave("caja", "nueva-clave", "clave-local", "clave-local"));

        Assert.Equal("No se puede cambiar la clave de ese usuario.", error.Message);
    }

    [Fact]
    public void Cambiar_clave_sin_la_clave_maestra_no_pasa()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarCambioClave(Roles.Admin, "nueva-clave", "otra", "clave-local"));

        Assert.Equal("La clave secreta no coincide.", error.Message);
    }

    [Fact]
    public void El_alta_crea_un_dueno_con_mail_y_clave()
    {
        var dueno = ServicioAdmin.NuevoDueno("Ana Perez", "Ana@Mail.com", "anap", "clave-123");

        Assert.Equal(Roles.Dueno, dueno.Rol);
        Assert.Equal("ana@mail.com", dueno.Email);
        Assert.Equal("anap", dueno.NombreUsuario);
        Assert.True(dueno.Activo);
    }

    [Fact]
    public void Un_dueno_con_clave_corta_no_se_crea()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.NuevoDueno("Ana", "ana@mail.com", "anap", "corta"));

        Assert.Equal("La contraseña debe tener al menos 8 caracteres.", error.Message);
    }

    [Fact]
    public void Desactivar_un_local_no_borra_su_nombre_ni_su_vencimiento()
    {
        var vence = new DateTimeOffset(2026, 11, 20, 15, 0, 0, TimeSpan.Zero);
        var local = new Local { Nombre = "Almagro", Vence = vence, Activo = true };

        ServicioAdmin.Desactivar(local);

        Assert.False(local.Activo);
        Assert.Equal("Almagro", local.Nombre);
        Assert.Equal(vence, local.Vence);
    }

    [Fact]
    public void Un_local_desactivado_se_puede_volver_a_activar()
    {
        var local = new Local { Activo = false };

        ServicioAdmin.Activar(local);

        Assert.True(local.Activo);
    }

    [Fact]
    public void Los_empleados_de_un_local_no_cuentan_en_otro()
    {
        (int IdLocal, string Rol)[] vinculos =
        [
            (1, Roles.Dueno),
            (1, Roles.Empleado),
            (1, Roles.Socio),
            (2, Roles.Empleado)
        ];

        Assert.Equal(2, ServicioAdmin.Empleados(vinculos, 1));
        Assert.Equal(1, ServicioAdmin.Empleados(vinculos, 2));
    }

    [Fact]
    public void El_telefono_de_ventas_se_guarda_y_se_muestra()
    {
        var guardado = ServicioAdmin.GuardarTelefonoVentas("+54 9 11 5883-1829");

        Assert.Equal("5491158831829", guardado);
        Assert.Equal("+54 9 11 5883-1829", ServicioAdmin.MostrarTelefono(guardado));
    }

    [Fact]
    public void El_mail_de_contacto_se_guarda_en_minusculas()
    {
        Assert.Equal("contacto@stockcontrol.com", ServicioAdmin.GuardarEmailVentas("Contacto@StockControl.com"));
    }

    [Fact]
    public void Un_mail_invalido_no_se_guarda()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioAdmin.GuardarEmailVentas("sin-arroba"));
    }

    [Fact]
    public void Contratar_otro_local_en_prueba_no_abre_un_trial()
    {
        var ahora = new DateTimeOffset(2026, 10, 20, 15, 0, 0, TimeSpan.Zero);
        var primero = ServicioAdmin.NuevoLocal("Uno", ahora);
        var otro = ServicioAdmin.ContratarLocal("Dos");
        var estado = ServicioAbono.Estado(otro, ahora);

        Assert.Equal(ServicioAbono.Trial, primero.EstadoAbono);
        Assert.Equal(ServicioAbono.Pendiente, otro.EstadoAbono);
        Assert.Null(otro.Vence);
        Assert.Equal(ServicioAbono.Pendiente, estado.Estado);
        Assert.False(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Nombre_vacio_no_crea_el_local()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ContratarLocal("   "));

        Assert.Equal("El nombre del local es obligatorio.", error.Message);
    }

    [Fact]
    public void Un_canal_de_pago_desconocido_no_pasa()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAdmin.ValidarCanal("cripto"));

        Assert.Equal("Elegí transferencia, efectivo o hablé con ventas.", error.Message);
    }
}
