using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121120000_AgregarVentaSinConexion")]
    public partial class AgregarVentaSinConexion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdVentaCliente",
                table: "InformeVenta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StockEnConflicto",
                table: "InformeVenta",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NotaRevision",
                table: "InformeVenta",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InformeVenta_IdLocal_IdVentaCliente",
                table: "InformeVenta",
                columns: new[] { "IdLocal", "IdVentaCliente" },
                unique: true,
                filter: "\"IdVentaCliente\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InformeVenta_IdLocal_IdVentaCliente",
                table: "InformeVenta");

            migrationBuilder.DropColumn(name: "NotaRevision", table: "InformeVenta");
            migrationBuilder.DropColumn(name: "StockEnConflicto", table: "InformeVenta");
            migrationBuilder.DropColumn(name: "IdVentaCliente", table: "InformeVenta");
        }
    }
}
