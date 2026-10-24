using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261024150000_PacksProveedor")]
    public partial class PacksProveedor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombrePack",
                table: "OrdenesCompraLineas",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UnidadesPack",
                table: "OrdenesCompraLineas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ProductosProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    IdProveedor = table.Column<int>(type: "integer", nullable: false),
                    NombrePack = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UnidadesPack = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosProveedor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductosProveedor_IdLocal_IdProveedor_IdProducto",
                table: "ProductosProveedor",
                columns: new[] { "IdLocal", "IdProveedor", "IdProducto" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductosProveedor");
            migrationBuilder.DropColumn(name: "NombrePack", table: "OrdenesCompraLineas");
            migrationBuilder.DropColumn(name: "UnidadesPack", table: "OrdenesCompraLineas");
        }
    }
}
