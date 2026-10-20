using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Abonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstadoSuscripcion",
                table: "Locales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdSuscripcionMp",
                table: "Locales",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Abonos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IdPreferenciaMp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UrlPago = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdPagoMp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaAcreditacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VenceAnterior = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VenceNuevo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abonos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true,
                filter: "\"Email\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Locales_IdSuscripcionMp",
                table: "Locales",
                column: "IdSuscripcionMp",
                unique: true,
                filter: "\"IdSuscripcionMp\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Abonos_IdLocal_Estado_FechaCreacion",
                table: "Abonos",
                columns: new[] { "IdLocal", "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Abonos_IdPagoMp",
                table: "Abonos",
                column: "IdPagoMp",
                unique: true,
                filter: "\"IdPagoMp\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abonos");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Locales_IdSuscripcionMp",
                table: "Locales");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EstadoSuscripcion",
                table: "Locales");

            migrationBuilder.DropColumn(
                name: "IdSuscripcionMp",
                table: "Locales");
        }
    }
}
