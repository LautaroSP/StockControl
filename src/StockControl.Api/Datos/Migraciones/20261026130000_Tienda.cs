using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261026130000_Tienda")]
    public partial class Tienda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AliasTienda",
                table: "Locales",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoTienda",
                table: "Locales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "inactiva");

            foreach (var columna in new[] { "Direccion", "Horario" })
                migrationBuilder.AddColumn<string>(
                    name: columna,
                    table: "Locales",
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false,
                    defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Locales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HaceEnvios",
                table: "Locales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            foreach (var columna in new[] { "Latitud", "Longitud" })
                migrationBuilder.AddColumn<decimal>(
                    name: columna,
                    table: "Locales",
                    type: "numeric(9,6)",
                    precision: 9,
                    scale: 6,
                    nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locales_AliasTienda",
                table: "Locales",
                column: "AliasTienda",
                unique: true,
                filter: "\"AliasTienda\" IS NOT NULL");

            migrationBuilder.AddColumn<bool>(
                name: "Publicado",
                table: "Productos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdLocal_Publicado",
                table: "Productos",
                columns: new[] { "IdLocal", "Publicado" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Productos_IdLocal_Publicado", table: "Productos");
            migrationBuilder.DropColumn(name: "Publicado", table: "Productos");
            migrationBuilder.DropIndex(name: "IX_Locales_AliasTienda", table: "Locales");
            foreach (var columna in new[] { "AliasTienda", "EstadoTienda", "Direccion", "Horario", "WhatsApp", "HaceEnvios", "Latitud", "Longitud" })
                migrationBuilder.DropColumn(name: columna, table: "Locales");
        }
    }
}
