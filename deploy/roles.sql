-- Roles de StockControl. Correr con psql, conectado a la base, como superusuario.
-- No guarda claves: después, \password stockcontrol_migracion y \password stockcontrol_app.

SELECT 'CREATE ROLE stockcontrol_migracion WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS'
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'stockcontrol_migracion')\gexec

SELECT 'CREATE ROLE stockcontrol_app WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS'
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'stockcontrol_app')\gexec

ALTER ROLE stockcontrol_migracion WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;
ALTER ROLE stockcontrol_app WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;

DO $$
BEGIN
  EXECUTE format('ALTER DATABASE %I OWNER TO stockcontrol_migracion', current_database());
  EXECUTE format(
    'GRANT CONNECT ON DATABASE %I TO stockcontrol_migracion, stockcontrol_app',
    current_database());
END $$;

REVOKE CREATE ON SCHEMA public FROM PUBLIC;
GRANT USAGE, CREATE ON SCHEMA public TO stockcontrol_migracion;
GRANT USAGE ON SCHEMA public TO stockcontrol_app;
ALTER SCHEMA public OWNER TO stockcontrol_migracion;

DO $$
DECLARE
  objeto record;
BEGIN
  FOR objeto IN
    SELECT c.relkind, c.relname
    FROM pg_class c
    JOIN pg_namespace n ON n.oid = c.relnamespace
    WHERE n.nspname = 'public' AND c.relkind IN ('r', 'p', 'v', 'S')
  LOOP
    IF objeto.relkind = 'S' THEN
      EXECUTE format('ALTER SEQUENCE public.%I OWNER TO stockcontrol_migracion', objeto.relname);
    ELSIF objeto.relkind = 'v' THEN
      EXECUTE format('ALTER VIEW public.%I OWNER TO stockcontrol_migracion', objeto.relname);
    ELSE
      EXECUTE format('ALTER TABLE public.%I OWNER TO stockcontrol_migracion', objeto.relname);
    END IF;
  END LOOP;
END $$;

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO stockcontrol_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO stockcontrol_app;

ALTER DEFAULT PRIVILEGES FOR ROLE stockcontrol_migracion IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO stockcontrol_app;
ALTER DEFAULT PRIVILEGES FOR ROLE stockcontrol_migracion IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO stockcontrol_app;
