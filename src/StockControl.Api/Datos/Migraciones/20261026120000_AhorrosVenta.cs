using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261026120000_AhorrosVenta")]
    public partial class AhorrosVenta : Migration
    {
        private static readonly string[] Tablas = ["InformeVentaDetalle", "LineasCuenta"];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var tabla in Tablas)
            {
                migrationBuilder.AddColumn<decimal>(
                    name: "PrecioBase",
                    table: tabla,
                    type: "numeric(14,2)",
                    precision: 14,
                    scale: 2,
                    nullable: true);

                foreach (var ahorro in new[] { "AhorroOferta", "AhorroDescuento", "AhorroCombo" })
                    migrationBuilder.AddColumn<decimal>(
                        name: ahorro,
                        table: tabla,
                        type: "numeric(14,2)",
                        precision: 14,
                        scale: 2,
                        nullable: false,
                        defaultValue: 0m);

                migrationBuilder.AddColumn<string>(
                    name: "NombreCombo",
                    table: tabla,
                    type: "character varying(60)",
                    maxLength: 60,
                    nullable: false,
                    defaultValue: "");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var tabla in Tablas)
                foreach (var columna in new[] { "PrecioBase", "AhorroOferta", "AhorroDescuento", "AhorroCombo", "NombreCombo" })
                    migrationBuilder.DropColumn(name: columna, table: tabla);
        }
    }
}
