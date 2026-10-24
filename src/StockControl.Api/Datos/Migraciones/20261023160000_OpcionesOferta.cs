using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261023160000_OpcionesOferta")]
    public partial class OpcionesOferta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnulaConDescuentoCaja",
                table: "Ofertas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AnulaConAlCosto",
                table: "Ofertas",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteNoAplicarEnCaja",
                table: "Ofertas",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AnulaConDescuentoCaja", table: "Ofertas");
            migrationBuilder.DropColumn(name: "AnulaConAlCosto", table: "Ofertas");
            migrationBuilder.DropColumn(name: "PermiteNoAplicarEnCaja", table: "Ofertas");
        }
    }
}
