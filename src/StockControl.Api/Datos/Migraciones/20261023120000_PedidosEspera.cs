using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261023120000_PedidosEspera")]
    public partial class PedidosEspera : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidosEspera",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    NroCaja = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: true),
                    Nota = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DescuentoPorcentaje = table.Column<int>(type: "integer", nullable: false),
                    AlCosto = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosEspera", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineasPedidoEspera",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdPedidoEspera = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasPedidoEspera", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasPedidoEspera_PedidosEspera_IdPedidoEspera",
                        column: x => x.IdPedidoEspera,
                        principalTable: "PedidosEspera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosEspera_IdLocal_NroCaja_Fecha",
                table: "PedidosEspera",
                columns: new[] { "IdLocal", "NroCaja", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasPedidoEspera_IdPedidoEspera",
                table: "LineasPedidoEspera",
                column: "IdPedidoEspera");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LineasPedidoEspera");
            migrationBuilder.DropTable(name: "PedidosEspera");
        }
    }
}
