using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121100000_FacturaArca")]
    public partial class FacturaArca : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesFiscales",
                columns: table => new
                {
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Cuit = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    PuntoVenta = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FacturaGlobalPorDefecto = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesFiscales", x => x.IdLocal);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesFiscales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdCierre = table.Column<int>(type: "integer", nullable: true),
                    NroCaja = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PuntoVenta = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<long>(type: "bigint", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Cae = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: ""),
                    VencimientoCae = table.Column<DateOnly>(type: "date", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Alcance = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IdComprobanteOrigen = table.Column<int>(type: "integer", nullable: true),
                    IdInformeVenta = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesFiscales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesFiscalesVentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdComprobanteFiscal = table.Column<int>(type: "integer", nullable: false),
                    IdInformeVenta = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesFiscalesVentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesFiscalesVentas_ComprobantesFiscales_IdComprobanteFiscal",
                        column: x => x.IdComprobanteFiscal,
                        principalTable: "ComprobantesFiscales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesFiscales_IdLocal_IdCierre",
                table: "ComprobantesFiscales",
                columns: new[] { "IdLocal", "IdCierre" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesFiscales_IdLocal_Tipo_PuntoVenta_Numero",
                table: "ComprobantesFiscales",
                columns: new[] { "IdLocal", "Tipo", "PuntoVenta", "Numero" },
                unique: true,
                filter: "\"Numero\" > 0 AND \"Estado\" <> 'Rechazado'");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesFiscalesVentas_IdComprobanteFiscal",
                table: "ComprobantesFiscalesVentas",
                column: "IdComprobanteFiscal");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesFiscalesVentas_IdInformeVenta",
                table: "ComprobantesFiscalesVentas",
                column: "IdInformeVenta",
                unique: true);

            // RLS: mismo aislamiento por local que el resto de las tablas con IdLocal.
            migrationBuilder.Sql(SqlPoliticas);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ComprobantesFiscalesVentas");
            migrationBuilder.DropTable(name: "ComprobantesFiscales");
            migrationBuilder.DropTable(name: "ConfiguracionesFiscales");
        }

        private const string SqlPoliticas = """
            DO $$
            DECLARE
              tabla text;
            BEGIN
              FOREACH tabla IN ARRAY ARRAY['ConfiguracionesFiscales', 'ComprobantesFiscales', 'ComprobantesFiscalesVentas']
              LOOP
                EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', tabla);
                EXECUTE format('DROP POLICY IF EXISTS aislamiento_local ON %I', tabla);
                EXECUTE format($pol$
                  CREATE POLICY aislamiento_local ON %I
                  USING (
                    current_setting('app.alcance', true) = 'todo'
                    OR "IdLocal" = NULLIF(current_setting('app.id_local', true), '')::int
                    OR "IdLocal" = ANY (COALESCE(string_to_array(NULLIF(current_setting('app.locales', true), ''), ','), ARRAY[]::text[])::int[])
                  )
                  WITH CHECK (
                    current_setting('app.alcance', true) = 'todo'
                    OR "IdLocal" = NULLIF(current_setting('app.id_local', true), '')::int
                  )
                $pol$, tabla);
              END LOOP;
            END $$;
            """;
    }
}
