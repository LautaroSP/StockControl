using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;
using StockControl.Dominio;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <summary>Gestión configurable por local desde el admin. Por defecto $ 500.</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121225000_ImporteGestionLocal")]
    public partial class ImporteGestionLocal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ImporteGestion",
                table: "Locales",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: ServicioGestionTienda.ImportePorDefecto);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImporteGestion",
                table: "Locales");
        }
    }
}
