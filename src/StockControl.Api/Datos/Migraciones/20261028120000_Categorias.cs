using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261028120000_Categorias")]
    public partial class Categorias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Categorias", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_IdLocal",
                table: "Categorias",
                column: "IdLocal");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Categorias_IdLocal_Nombre\" ON \"Categorias\" (\"IdLocal\", lower(\"Nombre\"));");

            migrationBuilder.AddColumn<int>(
                name: "IdCategoria",
                table: "Productos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCategoria",
                table: "GrupoProductos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdCategoria",
                table: "Productos",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoProductos_IdCategoria",
                table: "GrupoProductos",
                column: "IdCategoria");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_IdCategoria",
                table: "Productos",
                column: "IdCategoria",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoProductos_Categorias_IdCategoria",
                table: "GrupoProductos",
                column: "IdCategoria",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Productos_Categorias_IdCategoria", table: "Productos");
            migrationBuilder.DropForeignKey(name: "FK_GrupoProductos_Categorias_IdCategoria", table: "GrupoProductos");
            migrationBuilder.DropIndex(name: "IX_Productos_IdCategoria", table: "Productos");
            migrationBuilder.DropIndex(name: "IX_GrupoProductos_IdCategoria", table: "GrupoProductos");
            migrationBuilder.DropColumn(name: "IdCategoria", table: "Productos");
            migrationBuilder.DropColumn(name: "IdCategoria", table: "GrupoProductos");
            migrationBuilder.DropTable(name: "Categorias");
        }
    }
}
