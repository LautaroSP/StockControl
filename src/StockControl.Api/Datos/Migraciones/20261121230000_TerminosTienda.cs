using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <summary>Aceptación de términos de la tienda online por local.</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121230000_TerminosTienda")]
    public partial class TerminosTienda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AceptoTerminosTiendaEn",
                table: "Locales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionTerminosTienda",
                table: "Locales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AceptoTerminosTiendaEn", table: "Locales");
            migrationBuilder.DropColumn(name: "VersionTerminosTienda", table: "Locales");
        }
    }
}
