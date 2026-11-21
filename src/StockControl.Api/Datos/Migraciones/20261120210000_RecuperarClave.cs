using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261120210000_RecuperarClave")]
    public partial class RecuperarClave : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenRecuperacion",
                table: "Usuarios",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VenceRecuperacion",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TokenRecuperacion",
                table: "Usuarios",
                column: "TokenRecuperacion",
                unique: true,
                filter: "\"TokenRecuperacion\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Usuarios_TokenRecuperacion", table: "Usuarios");
            migrationBuilder.DropColumn(name: "TokenRecuperacion", table: "Usuarios");
            migrationBuilder.DropColumn(name: "VenceRecuperacion", table: "Usuarios");
        }
    }
}
