using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261101223000_AuditoriaSeguridad")]
    public partial class AuditoriaSeguridad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BloqueadoHasta",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(SqlPoliticas);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                  tabla text;
                BEGIN
                  FOR tabla IN
                    SELECT c.relname
                    FROM pg_class c
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE n.nspname = 'public' AND c.relkind = 'r' AND c.relrowsecurity
                  LOOP
                    EXECUTE format('DROP POLICY IF EXISTS aislamiento_local ON %I', tabla);
                    EXECUTE format('DROP POLICY IF EXISTS aislamiento_local_fila ON %I', tabla);
                    EXECUTE format('DROP POLICY IF EXISTS aislamiento_vinculo ON %I', tabla);
                    EXECUTE format('DROP POLICY IF EXISTS ajustes_plataforma ON %I', tabla);
                    EXECUTE format('DROP POLICY IF EXISTS mensajes_leer ON %I', tabla);
                    EXECUTE format('DROP POLICY IF EXISTS mensajes_insertar ON %I', tabla);
                    EXECUTE format('DROP POLICY IF EXISTS mensajes_cambiar ON %I', tabla);
                    EXECUTE format('DROP POLICY IF EXISTS mensajes_borrar ON %I', tabla);
                    EXECUTE format('ALTER TABLE %I DISABLE ROW LEVEL SECURITY', tabla);
                  END LOOP;
                END $$;
                DROP FUNCTION IF EXISTS tienda_por_alias(text);
                DROP FUNCTION IF EXISTS alias_tomado(text, integer);
                DROP FUNCTION IF EXISTS ajuste_publico(text);
                DROP FUNCTION IF EXISTS locales_del_usuario(integer);
                """);

            migrationBuilder.DropColumn(name: "BloqueadoHasta", table: "Usuarios");
            migrationBuilder.DropColumn(name: "IntentosFallidos", table: "Usuarios");
        }

        private const string SqlPoliticas = """
            CREATE OR REPLACE FUNCTION locales_del_usuario(id_usuario integer)
            RETURNS SETOF integer
            LANGUAGE sql
            STABLE
            SECURITY DEFINER
            SET search_path = public
            AS $$
              SELECT "IdLocal" FROM "UsuarioLocales" WHERE "IdUsuario" = id_usuario
            $$;

            CREATE OR REPLACE FUNCTION tienda_por_alias(alias text)
            RETURNS integer
            LANGUAGE sql
            STABLE
            SECURITY DEFINER
            SET search_path = public
            AS $$
              SELECT "IdLocal" FROM "Locales" WHERE "AliasTienda" = alias LIMIT 1
            $$;

            CREATE OR REPLACE FUNCTION alias_tomado(alias text, id_actual integer)
            RETURNS boolean
            LANGUAGE sql
            STABLE
            SECURITY DEFINER
            SET search_path = public
            AS $$
              SELECT EXISTS (
                SELECT 1 FROM "Locales"
                WHERE "AliasTienda" = alias AND "IdLocal" <> id_actual
              )
            $$;

            CREATE OR REPLACE FUNCTION ajuste_publico(clave text)
            RETURNS text
            LANGUAGE sql
            STABLE
            SECURITY DEFINER
            SET search_path = public
            AS $$
              SELECT "Valor" FROM "Ajustes"
              WHERE "Clave" = clave
                AND clave IN ('TelefonoVentas', 'EmailVentas', 'FormularioContacto')
            $$;

            GRANT EXECUTE ON FUNCTION locales_del_usuario(integer) TO PUBLIC;
            GRANT EXECUTE ON FUNCTION tienda_por_alias(text) TO PUBLIC;
            GRANT EXECUTE ON FUNCTION alias_tomado(text, integer) TO PUBLIC;
            GRANT EXECUTE ON FUNCTION ajuste_publico(text) TO PUBLIC;

            DO $$
            DECLARE
              tabla text;
            BEGIN
              FOR tabla IN
                SELECT c.relname
                FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                JOIN pg_attribute a ON a.attrelid = c.oid AND a.attname = 'IdLocal' AND NOT a.attisdropped
                WHERE n.nspname = 'public' AND c.relkind = 'r'
                  AND c.relname NOT IN ('Locales', 'UsuarioLocales')
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

            ALTER TABLE "Locales" ENABLE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS aislamiento_local_fila ON "Locales";
            CREATE POLICY aislamiento_local_fila ON "Locales"
            USING (
              current_setting('app.alcance', true) = 'todo'
              OR "IdLocal" = NULLIF(current_setting('app.id_local', true), '')::int
              OR "IdLocal" = ANY (COALESCE(string_to_array(NULLIF(current_setting('app.locales', true), ''), ','), ARRAY[]::text[])::int[])
              OR "IdLocal" IN (SELECT locales_del_usuario(NULLIF(current_setting('app.id_usuario', true), '')::int))
            )
            WITH CHECK (
              current_setting('app.alcance', true) = 'todo'
              OR "IdLocal" = NULLIF(current_setting('app.id_local', true), '')::int
            );

            ALTER TABLE "UsuarioLocales" ENABLE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS aislamiento_vinculo ON "UsuarioLocales";
            CREATE POLICY aislamiento_vinculo ON "UsuarioLocales"
            USING (
              current_setting('app.alcance', true) = 'todo'
              OR "IdUsuario" = NULLIF(current_setting('app.id_usuario', true), '')::int
              OR "IdLocal" = NULLIF(current_setting('app.id_local', true), '')::int
              OR "IdLocal" IN (SELECT locales_del_usuario(NULLIF(current_setting('app.id_usuario', true), '')::int))
            )
            WITH CHECK (
              current_setting('app.alcance', true) = 'todo'
              OR "IdLocal" = NULLIF(current_setting('app.id_local', true), '')::int
              OR "IdLocal" IN (SELECT locales_del_usuario(NULLIF(current_setting('app.id_usuario', true), '')::int))
            );

            ALTER TABLE "Ajustes" ENABLE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS ajustes_plataforma ON "Ajustes";
            CREATE POLICY ajustes_plataforma ON "Ajustes"
            USING (current_setting('app.alcance', true) = 'todo')
            WITH CHECK (current_setting('app.alcance', true) = 'todo');

            ALTER TABLE "MensajesContacto" ENABLE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS mensajes_leer ON "MensajesContacto";
            DROP POLICY IF EXISTS mensajes_insertar ON "MensajesContacto";
            DROP POLICY IF EXISTS mensajes_cambiar ON "MensajesContacto";
            DROP POLICY IF EXISTS mensajes_borrar ON "MensajesContacto";
            CREATE POLICY mensajes_leer ON "MensajesContacto"
              FOR SELECT USING (current_setting('app.alcance', true) = 'todo');
            CREATE POLICY mensajes_insertar ON "MensajesContacto"
              FOR INSERT WITH CHECK (true);
            CREATE POLICY mensajes_cambiar ON "MensajesContacto"
              FOR UPDATE
              USING (current_setting('app.alcance', true) = 'todo')
              WITH CHECK (current_setting('app.alcance', true) = 'todo');
            CREATE POLICY mensajes_borrar ON "MensajesContacto"
              FOR DELETE USING (current_setting('app.alcance', true) = 'todo');
            """;
    }
}
