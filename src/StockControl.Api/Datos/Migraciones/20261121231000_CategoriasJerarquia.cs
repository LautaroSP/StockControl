using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121231000_CategoriasJerarquia")]
    public partial class CategoriasJerarquia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdPadre",
                table: "Categorias",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_IdPadre",
                table: "Categorias",
                column: "IdPadre");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_IdPadre",
                table: "Categorias",
                column: "IdPadre",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<string>(
                name: "FiltroCategoriasTienda",
                table: "Locales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "chips");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Categorias_Categorias_IdPadre", table: "Categorias");
            migrationBuilder.DropIndex(name: "IX_Categorias_IdPadre", table: "Categorias");
            migrationBuilder.DropColumn(name: "IdPadre", table: "Categorias");
            migrationBuilder.DropColumn(name: "FiltroCategoriasTienda", table: "Locales");
        }
    }
}
