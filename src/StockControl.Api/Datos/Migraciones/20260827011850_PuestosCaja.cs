using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PuestosCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCierre",
                table: "InformeVenta",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCierre",
                table: "Cajas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoDesglose",
                table: "Cajas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Medio");

            // Historial: IdCierre = viejo correlativo; NroCaja (puesto) = 1.
            migrationBuilder.Sql("""
                UPDATE "Cajas" SET "IdCierre" = "NroCaja", "TipoDesglose" = 'Medio';
                UPDATE "InformeVenta" SET "IdCierre" = "NroCaja" WHERE "NroCaja" IS NOT NULL;
                UPDATE "InformeVenta" SET "NroCaja" = 1;
                UPDATE "Cajas" SET "NroCaja" = 1;
                INSERT INTO "Configuracion" ("IdLocal", "Clave", "Valor")
                SELECT l."IdLocal", 'CantidadCajas', '1'
                FROM "Locales" l
                WHERE NOT EXISTS (
                    SELECT 1 FROM "Configuracion" c
                    WHERE c."IdLocal" = l."IdLocal" AND c."Clave" = 'CantidadCajas');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "NroCaja",
                table: "InformeVenta",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OcupacionesCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    NroCaja = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    NombreUsuario = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Desde = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OcupacionesCaja", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_IdLocal_IdCierre",
                table: "Cajas",
                columns: new[] { "IdLocal", "IdCierre" });

            migrationBuilder.CreateIndex(
                name: "IX_OcupacionesCaja_IdLocal_NroCaja_IdUsuario",
                table: "OcupacionesCaja",
                columns: new[] { "IdLocal", "NroCaja", "IdUsuario" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OcupacionesCaja");

            migrationBuilder.DropIndex(
                name: "IX_Cajas_IdLocal_IdCierre",
                table: "Cajas");

            migrationBuilder.Sql("""
                UPDATE "InformeVenta" SET "NroCaja" = "IdCierre" WHERE "IdCierre" IS NOT NULL;
                UPDATE "InformeVenta" SET "NroCaja" = NULL WHERE "IdCierre" IS NULL;
                UPDATE "Cajas" SET "NroCaja" = "IdCierre";
                DELETE FROM "Configuracion" WHERE "Clave" = 'CantidadCajas';
                """);

            migrationBuilder.DropColumn(
                name: "IdCierre",
                table: "InformeVenta");

            migrationBuilder.DropColumn(
                name: "IdCierre",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "TipoDesglose",
                table: "Cajas");

            migrationBuilder.AlterColumn<int>(
                name: "NroCaja",
                table: "InformeVenta",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
