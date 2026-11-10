#!/bin/bash
# Runs once, on first initialization of an empty data volume (docker-entrypoint-initdb.d).
# Creates the two application roles required by docs/security/TENANT_ISOLATION.md (layer 4, RLS):
#   doubleus_owner : owns the schema and runs migrations (DDL)
#   doubleus_app   : runtime role; not owner, not superuser, NOBYPASSRLS, so row-level security always applies
# Passwords come from the container environment; nothing is hard-coded here.
set -euo pipefail

: "${DOUBLEUS_OWNER_PASSWORD:?missing}"
: "${DOUBLEUS_APP_PASSWORD:?missing}"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" \
  -v owner_pw="$DOUBLEUS_OWNER_PASSWORD" -v app_pw="$DOUBLEUS_APP_PASSWORD" <<'SQL'
CREATE ROLE doubleus_owner LOGIN PASSWORD :'owner_pw' NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;
CREATE ROLE doubleus_app   LOGIN PASSWORD :'app_pw'   NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;

-- Extensions need superuser to create; migrations (running as doubleus_owner) rely on them already existing.
CREATE EXTENSION IF NOT EXISTS vector;

ALTER SCHEMA public OWNER TO doubleus_owner;
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO doubleus_app;

-- Objects created later by doubleus_owner (migrations) become usable by the runtime role, DML only.
ALTER DEFAULT PRIVILEGES FOR ROLE doubleus_owner IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO doubleus_app;
ALTER DEFAULT PRIVILEGES FOR ROLE doubleus_owner IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO doubleus_app;

REVOKE ALL ON DATABASE :"DBNAME" FROM PUBLIC;
GRANT CONNECT ON DATABASE :"DBNAME" TO doubleus_owner, doubleus_app;
SQL
