using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261120180000_DescuentoMedio")]
    public partial class DescuentoMedio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DescuentoPorcentaje",
                table: "MetodosPago",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoMedio",
                table: "InformeVenta",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DescuentoMedioPorcentaje",
                table: "InformeVenta",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DescuentoPorcentaje", table: "MetodosPago");
            migrationBuilder.DropColumn(name: "DescuentoMedio", table: "InformeVenta");
            migrationBuilder.DropColumn(name: "DescuentoMedioPorcentaje", table: "InformeVenta");
        }
    }
}
