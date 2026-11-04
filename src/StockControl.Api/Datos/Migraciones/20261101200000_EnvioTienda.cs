using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class EnvioTienda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EnvioGratisDesde",
                table: "Locales",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnvioGratisSiempre",
                table: "Locales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "EnvioMinimo",
                table: "Locales",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioEnvioKm",
                table: "Locales",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RadioEnvioKm",
                table: "Locales",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionEnvio",
                table: "PedidosTienda",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DistanciaKm",
                table: "PedidosTienda",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Envio",
                table: "PedidosTienda",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LatitudEnvio",
                table: "PedidosTienda",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LongitudEnvio",
                table: "PedidosTienda",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modalidad",
                table: "PedidosTienda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "retiro");

            migrationBuilder.AddColumn<string>(
                name: "OrigenDistancia",
                table: "PedidosTienda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var columna in new[] { "EnvioGratisDesde", "EnvioGratisSiempre", "EnvioMinimo", "PrecioEnvioKm", "RadioEnvioKm" })
                migrationBuilder.DropColumn(name: columna, table: "Locales");

            foreach (var columna in new[] { "DireccionEnvio", "DistanciaKm", "Envio", "LatitudEnvio", "LongitudEnvio", "Modalidad", "OrigenDistancia" })
                migrationBuilder.DropColumn(name: columna, table: "PedidosTienda");
        }
    }
}
