using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class MediosPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "MetodosPago",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "PagosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdInformeVenta = table.Column<int>(type: "integer", nullable: false),
                    IdMetodoPago = table.Column<int>(type: "integer", nullable: false),
                    DescripcionMetodoPago = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosVenta_InformeVenta_IdInformeVenta",
                        column: x => x.IdInformeVenta,
                        principalTable: "InformeVenta",
                        principalColumn: "IdInformeVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosVenta_IdInformeVenta",
                table: "PagosVenta",
                column: "IdInformeVenta");

            migrationBuilder.CreateIndex(
                name: "IX_PagosVenta_IdLocal_IdInformeVenta",
                table: "PagosVenta",
                columns: new[] { "IdLocal", "IdInformeVenta" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagosVenta");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "MetodosPago",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);
        }
    }
}
