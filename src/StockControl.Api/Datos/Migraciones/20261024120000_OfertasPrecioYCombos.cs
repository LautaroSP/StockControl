using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261024120000_OfertasPrecioYCombos")]
    public partial class OfertasPrecioYCombos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnidad",
                table: "Ofertas",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "OfertasCombo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    FechaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaHasta = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfertasCombo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineasOfertaCombo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdOfertaCombo = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasOfertaCombo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasOfertaCombo_OfertasCombo_IdOfertaCombo",
                        column: x => x.IdOfertaCombo,
                        principalTable: "OfertasCombo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfertasCombo_IdLocal_FechaDesde",
                table: "OfertasCombo",
                columns: new[] { "IdLocal", "FechaDesde" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasOfertaCombo_IdOfertaCombo",
                table: "LineasOfertaCombo",
                column: "IdOfertaCombo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LineasOfertaCombo");
            migrationBuilder.DropTable(name: "OfertasCombo");
            migrationBuilder.DropColumn(name: "PrecioUnidad", table: "Ofertas");
        }
    }
}
