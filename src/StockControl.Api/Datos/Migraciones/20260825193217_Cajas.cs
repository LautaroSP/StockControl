using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Cajas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NroCaja",
                table: "InformeVenta",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    IdCaja = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    NroCaja = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CantidadVentas = table.Column<int>(type: "integer", nullable: false),
                    IdUsuarioCierre = table.Column<int>(type: "integer", nullable: true),
                    NombreCierre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.IdCaja);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InformeVenta_IdLocal_Fecha",
                table: "InformeVenta",
                columns: new[] { "IdLocal", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_IdLocal_NroCaja",
                table: "Cajas",
                columns: new[] { "IdLocal", "NroCaja" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropIndex(
                name: "IX_InformeVenta_IdLocal_Fecha",
                table: "InformeVenta");

            migrationBuilder.DropColumn(
                name: "NroCaja",
                table: "InformeVenta");
        }
    }
}
