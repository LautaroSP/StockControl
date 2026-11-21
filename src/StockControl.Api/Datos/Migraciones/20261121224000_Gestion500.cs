using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <summary>Baja la gestión de $ 1.000 a $ 500 en pedidos/deudas pendientes aún no cobradas.</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121224000_Gestion500")]
    public partial class Gestion500 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "PedidosTienda"
                SET "Gestion" = 500,
                    "Total" = CASE
                        WHEN "GestionACargoComprador" THEN "Subtotal" + "Envio" + 500
                        ELSE "Subtotal" + "Envio"
                    END
                WHERE "Gestion" = 1000
                  AND "Estado" <> 'cancelado'
                  AND ("EstadoPago" IS NULL OR "EstadoPago" NOT IN ('pagado', 'reembolsado'));
                """);

            migrationBuilder.Sql("""
                UPDATE "GestionesTienda"
                SET "Importe" = 500
                WHERE "Estado" = 'pendiente' AND "Importe" = 1000;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
