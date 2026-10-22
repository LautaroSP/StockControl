using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261022160000_ConfirmacionCorreo")]
    public partial class ConfirmacionCorreo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmado",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenConfirmacion",
                table: "Usuarios",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VenceConfirmacion",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TokenConfirmacion",
                table: "Usuarios",
                column: "TokenConfirmacion",
                unique: true,
                filter: "\"TokenConfirmacion\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Usuarios_TokenConfirmacion", table: "Usuarios");
            migrationBuilder.DropColumn(name: "EmailConfirmado", table: "Usuarios");
            migrationBuilder.DropColumn(name: "TokenConfirmacion", table: "Usuarios");
            migrationBuilder.DropColumn(name: "VenceConfirmacion", table: "Usuarios");
        }
    }
}
