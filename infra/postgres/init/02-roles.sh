#!/bin/sh
# Crea los dos roles de PostgreSQL que exige docs/convenciones.md (seguridad > multi-empresa):
#
# 1. $TALLEROS_MIGRATOR_USER: dueño del esquema, corre las migraciones de EF Core.
#    Tiene permisos de DDL (crear/alterar tablas) pero NO es superusuario.
# 2. $TALLEROS_APP_USER: con el que se conecta la API en tiempo de ejecución.
#    Solo DML (select/insert/update/delete), sin DDL, sin BYPASSRLS. Si algún día
#    el owner de una tabla se conecta sin querer, Postgres SIEMPRE aplica RLS
#    para este rol porque no tiene ese privilegio (a diferencia del owner/superuser,
#    que se salta RLS salvo que la tabla use FORCE ROW LEVEL SECURITY).
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
	DO \$\$
	BEGIN
		IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '$TALLEROS_MIGRATOR_USER') THEN
			CREATE ROLE "$TALLEROS_MIGRATOR_USER" WITH LOGIN PASSWORD '$TALLEROS_MIGRATOR_PASSWORD' NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;
		END IF;
		IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '$TALLEROS_APP_USER') THEN
			CREATE ROLE "$TALLEROS_APP_USER" WITH LOGIN PASSWORD '$TALLEROS_APP_PASSWORD' NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;
		END IF;
	END
	\$\$;

	-- El migrador es dueño de la base: puede crear y alterar tablas (migraciones).
	ALTER DATABASE "$POSTGRES_DB" OWNER TO "$TALLEROS_MIGRATOR_USER";
	GRANT ALL PRIVILEGES ON DATABASE "$POSTGRES_DB" TO "$TALLEROS_MIGRATOR_USER";

	-- La app solo necesita conectarse y usar el esquema public; el DML sobre
	-- cada tabla se otorga en la migración que la crea (ALTER DEFAULT PRIVILEGES
	-- del migrador ya cubre las tablas futuras automáticamente).
	GRANT CONNECT ON DATABASE "$POSTGRES_DB" TO "$TALLEROS_APP_USER";
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
	\connect "$POSTGRES_DB"

	GRANT USAGE ON SCHEMA public TO "$TALLEROS_APP_USER";

	-- Para que las tablas que el migrador cree a partir de ahora ya queden
	-- con permisos de DML para la app sin tener que otorgarlos tabla por tabla.
	ALTER DEFAULT PRIVILEGES FOR ROLE "$TALLEROS_MIGRATOR_USER" IN SCHEMA public
		GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO "$TALLEROS_APP_USER";
	ALTER DEFAULT PRIVILEGES FOR ROLE "$TALLEROS_MIGRATOR_USER" IN SCHEMA public
		GRANT USAGE, SELECT ON SEQUENCES TO "$TALLEROS_APP_USER";
EOSQL
