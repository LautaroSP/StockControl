using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261026140000_ImagenesTienda")]
    public partial class ImagenesTienda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImagenesProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Archivo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TipoContenido = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Fuente = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Enlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Licencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ImagenesProducto", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesProducto_IdLocal_IdProducto_Orden",
                table: "ImagenesProducto",
                columns: new[] { "IdLocal", "IdProducto", "Orden" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "ImagenesRechazadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ImagenesRechazadas", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesRechazadas_IdLocal_IdProducto",
                table: "ImagenesRechazadas",
                columns: new[] { "IdLocal", "IdProducto" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ImagenesRechazadas");
            migrationBuilder.DropTable(name: "ImagenesProducto");
        }
    }
}
