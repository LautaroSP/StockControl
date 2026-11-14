using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261111143000_LegalesBajaYArrepentimiento")]
    public partial class LegalesBajaYArrepentimiento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Renueva",
                table: "Locales",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Arrepentimientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Local = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    Mensaje = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: ""),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pendiente"),
                    Creado = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdLocal = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrepentimientos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Arrepentimientos_Codigo",
                table: "Arrepentimientos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Arrepentimientos_IdLocal_Estado",
                table: "Arrepentimientos",
                columns: new[] { "IdLocal", "Estado" });

            migrationBuilder.Sql("""
                ALTER TABLE "Arrepentimientos" ENABLE ROW LEVEL SECURITY;
                DROP POLICY IF EXISTS arrepentimientos_leer ON "Arrepentimientos";
                DROP POLICY IF EXISTS arrepentimientos_insertar ON "Arrepentimientos";
                CREATE POLICY arrepentimientos_leer ON "Arrepentimientos"
                  FOR SELECT USING (
                    current_setting('app.alcance', true) = 'todo'
                    OR (
                      "IdLocal" IS NOT NULL AND (
                        "IdLocal" = NULLIF(current_setting('app.id_local', true), '')::int
                        OR "IdLocal" IN (SELECT locales_del_usuario(NULLIF(current_setting('app.id_usuario', true), '')::int))
                      )
                    )
                  );
                CREATE POLICY arrepentimientos_insertar ON "Arrepentimientos"
                  FOR INSERT WITH CHECK (true);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP POLICY IF EXISTS arrepentimientos_leer ON "Arrepentimientos"; DROP POLICY IF EXISTS arrepentimientos_insertar ON "Arrepentimientos";""");
            migrationBuilder.DropTable(name: "Arrepentimientos");
            migrationBuilder.DropColumn(name: "Renueva", table: "Locales");
        }
    }
}
