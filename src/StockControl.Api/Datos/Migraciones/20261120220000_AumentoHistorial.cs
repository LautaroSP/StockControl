using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261120220000_AumentoHistorial")]
    public partial class AumentoHistorial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AumentosHistorial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Usuario = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TipoPrecio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ValorPrecio = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    TipoCosto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ValorCosto = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    RedondeoPrecio = table.Column<int>(type: "integer", nullable: false),
                    Deshecho = table.Column<bool>(type: "boolean", nullable: false),
                    FechaDeshecho = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UsuarioDeshizo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AumentosHistorial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AumentosHistorialItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdAumentoHistorial = table.Column<int>(type: "integer", nullable: false),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: true),
                    IdGrupoProducto = table.Column<int>(type: "integer", nullable: true),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrecioAntes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioDespues = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoAntes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoDespues = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AumentosHistorialItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AumentosHistorialItems_AumentosHistorial_IdAumentoHistorial",
                        column: x => x.IdAumentoHistorial,
                        principalTable: "AumentosHistorial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AumentosHistorial_IdLocal_Fecha",
                table: "AumentosHistorial",
                columns: new[] { "IdLocal", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_AumentosHistorialItems_IdAumentoHistorial",
                table: "AumentosHistorialItems",
                column: "IdAumentoHistorial");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AumentosHistorialItems");
            migrationBuilder.DropTable(name: "AumentosHistorial");
        }
    }
}
