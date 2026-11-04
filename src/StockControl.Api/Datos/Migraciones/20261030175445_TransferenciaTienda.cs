using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class TransferenciaTienda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PedidosTienda_MedioPago_EstadoPago_Creado",
                table: "PedidosTienda");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VencePago",
                table: "PedidosTienda",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AliasCbuTienda",
                table: "Locales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CbuTienda",
                table: "Locales",
                type: "character varying(22)",
                maxLength: 22,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PlazoTransferenciaHoras",
                table: "Locales",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitularTienda",
                table: "Locales",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTienda_EstadoPago_VencePago",
                table: "PedidosTienda",
                columns: new[] { "EstadoPago", "VencePago" });

            migrationBuilder.Sql("UPDATE \"Locales\" SET \"PlazoTransferenciaHoras\" = 24;");
            migrationBuilder.Sql(
                "UPDATE \"PedidosTienda\" SET \"VencePago\" = \"Creado\" + interval '30 minutes' " +
                "WHERE \"MedioPago\" = 'mercadopago' AND \"EstadoPago\" = 'pendiente';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PedidosTienda_EstadoPago_VencePago",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "VencePago",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "AliasCbuTienda",
                table: "Locales");

            migrationBuilder.DropColumn(
                name: "CbuTienda",
                table: "Locales");

            migrationBuilder.DropColumn(
                name: "PlazoTransferenciaHoras",
                table: "Locales");

            migrationBuilder.DropColumn(
                name: "TitularTienda",
                table: "Locales");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTienda_MedioPago_EstadoPago_Creado",
                table: "PedidosTienda",
                columns: new[] { "MedioPago", "EstadoPago", "Creado" });
        }
    }
}
