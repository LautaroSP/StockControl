using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261111120000_TerminosAceptados")]
    public partial class TerminosAceptados : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AceptoTerminosEn",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionTerminos",
                table: "Usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceptoTerminosEn",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "VersionTerminos",
                table: "Usuarios");
        }
    }
}
