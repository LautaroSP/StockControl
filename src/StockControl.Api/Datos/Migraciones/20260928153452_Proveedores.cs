using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Proveedores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdProveedor",
                table: "Productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdProveedor = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: true),
                    NombreUsuario = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoProductos = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompraLineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdOrdenCompra = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompraLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraLineas_OrdenesCompra_IdOrdenCompra",
                        column: x => x.IdOrdenCompra,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_IdLocal_IdProveedor_Fecha",
                table: "OrdenesCompra",
                columns: new[] { "IdLocal", "IdProveedor", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraLineas_IdOrdenCompra",
                table: "OrdenesCompraLineas",
                column: "IdOrdenCompra");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenesCompraLineas");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "IdProveedor",
                table: "Productos");
        }
    }
}
