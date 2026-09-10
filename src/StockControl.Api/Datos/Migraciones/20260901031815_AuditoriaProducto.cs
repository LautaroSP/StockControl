using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AuditoriaProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioModificacion",
                table: "Productos",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "import");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioModificacion",
                table: "Productos");
        }
    }
}
