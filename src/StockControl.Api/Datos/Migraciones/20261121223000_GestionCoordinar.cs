using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <summary>
    /// Pedidos "al retirar" viejos quedaron con Gestión 0. Congela $ 1.000 (o el subtotal si era menor)
    /// y nace la deuda de los ya entregados.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121223000_GestionCoordinar")]
    public partial class GestionCoordinar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "PedidosTienda" p
                SET
                    "Gestion" = LEAST(1000, GREATEST(0, p."Subtotal")),
                    "GestionACargoComprador" = COALESCE(l."GestionACargoComprador", FALSE)
                FROM "Locales" l
                WHERE p."IdLocal" = l."IdLocal"
                  AND p."MedioPago" = 'coordinar'
                  AND p."Gestion" = 0
                  AND p."Subtotal" > 0
                  AND COALESCE(l."ExentoGestion", FALSE) = FALSE
                  AND p."Estado" <> 'cancelado';
                """);

            migrationBuilder.Sql("""
                INSERT INTO "GestionesTienda" ("IdLocal", "IdPedido", "Importe", "Medio", "Estado", "Nacida", "VencePeriodo")
                SELECT
                    p."IdLocal",
                    p."Id",
                    p."Gestion",
                    p."MedioPago",
                    'pendiente',
                    COALESCE(p."Entregado", NOW()),
                    COALESCE(l."Vence", COALESCE(p."Entregado", NOW()))
                FROM "PedidosTienda" p
                JOIN "Locales" l ON l."IdLocal" = p."IdLocal"
                WHERE p."MedioPago" = 'coordinar'
                  AND p."Estado" = 'entregado'
                  AND p."Gestion" > 0
                  AND NOT EXISTS (
                      SELECT 1 FROM "GestionesTienda" g WHERE g."IdPedido" = p."Id"
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
