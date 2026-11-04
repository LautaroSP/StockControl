using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class GestionesTienda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GestionesTienda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdPedido = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Medio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IdAbono = table.Column<int>(type: "integer", nullable: true),
                    Nacida = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaSaldo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VencePeriodo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GestionesTienda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GestionesTienda_Abonos_IdAbono",
                        column: x => x.IdAbono,
                        principalTable: "Abonos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GestionesTienda_PedidosTienda_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "PedidosTienda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GestionesTienda_IdAbono",
                table: "GestionesTienda",
                column: "IdAbono");

            migrationBuilder.CreateIndex(
                name: "IX_GestionesTienda_IdLocal_Estado",
                table: "GestionesTienda",
                columns: new[] { "IdLocal", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_GestionesTienda_IdPedido",
                table: "GestionesTienda",
                column: "IdPedido",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GestionesTienda");
        }
    }
}
