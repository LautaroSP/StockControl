using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PagosTienda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoPago",
                table: "PedidosTienda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "sinpago");

            migrationBuilder.AddColumn<decimal>(
                name: "Gestion",
                table: "PedidosTienda",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "GestionACargoComprador",
                table: "PedidosTienda",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IdPagoMp",
                table: "PedidosTienda",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedioPago",
                table: "PedidosTienda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "coordinar");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Pagado",
                table: "PedidosTienda",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlPagoMp",
                table: "PedidosTienda",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ExentoGestion",
                table: "Locales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GestionACargoComprador",
                table: "Locales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTienda_IdPagoMp",
                table: "PedidosTienda",
                column: "IdPagoMp",
                unique: true,
                filter: "\"IdPagoMp\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosTienda_MedioPago_EstadoPago_Creado",
                table: "PedidosTienda",
                columns: new[] { "MedioPago", "EstadoPago", "Creado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PedidosTienda_IdPagoMp",
                table: "PedidosTienda");

            migrationBuilder.DropIndex(
                name: "IX_PedidosTienda_MedioPago_EstadoPago_Creado",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "Gestion",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "GestionACargoComprador",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "IdPagoMp",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "MedioPago",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "Pagado",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "UrlPagoMp",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "ExentoGestion",
                table: "Locales");

            migrationBuilder.DropColumn(
                name: "GestionACargoComprador",
                table: "Locales");
        }
    }
}
