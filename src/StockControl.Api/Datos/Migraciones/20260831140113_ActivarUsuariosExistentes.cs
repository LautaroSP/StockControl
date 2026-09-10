using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ActivarUsuariosExistentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Usuarios\" SET \"Activo\" = TRUE WHERE \"Activo\" = FALSE;");
            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
