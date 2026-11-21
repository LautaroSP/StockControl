using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121220000_EsTiendaOnline")]
    public partial class EsTiendaOnline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsTiendaOnline",
                table: "InformeVenta",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "InformeVenta" v
                SET "EsTiendaOnline" = TRUE
                FROM "PedidosTienda" p
                WHERE p."IdInformeVenta" = v."IdInformeVenta";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_InformeVenta_IdLocal_EsTiendaOnline_Fecha",
                table: "InformeVenta",
                columns: new[] { "IdLocal", "EsTiendaOnline", "Fecha" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InformeVenta_IdLocal_EsTiendaOnline_Fecha",
                table: "InformeVenta");

            migrationBuilder.DropColumn(name: "EsTiendaOnline", table: "InformeVenta");
        }
    }
}
