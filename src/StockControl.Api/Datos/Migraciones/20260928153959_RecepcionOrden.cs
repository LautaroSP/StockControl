using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class RecepcionOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadRecibida",
                table: "OrdenesCompraLineas",
                type: "numeric(14,3)",
                precision: 14,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaRecepcion",
                table: "OrdenesCompra",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioRecepcion",
                table: "OrdenesCompra",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreUsuarioRecepcion",
                table: "OrdenesCompra",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadRecibida",
                table: "OrdenesCompraLineas");

            migrationBuilder.DropColumn(
                name: "FechaRecepcion",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "IdUsuarioRecepcion",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "NombreUsuarioRecepcion",
                table: "OrdenesCompra");
        }
    }
}
