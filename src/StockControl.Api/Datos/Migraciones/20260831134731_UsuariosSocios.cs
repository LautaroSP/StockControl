using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class UsuariosSocios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Usuarios");
        }
    }
}
