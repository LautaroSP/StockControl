using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PlantillasCartel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantillasCartel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Tamano = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TipoContenido = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Datos = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasCartel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasCartel_IdLocal_Tamano",
                table: "PlantillasCartel",
                columns: new[] { "IdLocal", "Tamano" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillasCartel");
        }
    }
}
