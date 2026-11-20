#!/bin/sh
# Inicializa el clúster de Garage de un solo nodo (se corre una sola vez,
# después de `docker compose up -d garage`). Es seguro volver a correrlo:
# cada paso revisa si ya está hecho antes de repetirlo.
#
# Uso: infra/garage/bootstrap.sh
set -eu
export MSYS_NO_PATHCONV=1 # evita que Git Bash en Windows reescriba /garage como ruta local

CONTAINER=talleros-garage-1
BUCKET=talleros

run() { docker exec "$CONTAINER" /garage "$@"; }

echo "Esperando a que Garage responda..."
for i in $(seq 1 20); do
	run status >/dev/null 2>&1 && break
	sleep 1
done

if run layout show | grep -q "No nodes currently have a role"; then
	NODE_ID=$(run status | awk '/NO ROLE ASSIGNED/ {print $1; exit}')
	if [ -z "${NODE_ID:-}" ]; then
		echo "No se pudo leer el node id de 'garage status'." >&2
		run status
		exit 1
	fi
	echo "Asignando layout de un solo nodo ($NODE_ID)..."
	run layout assign -z dev -c 1GB "$NODE_ID"
	CURRENT_VERSION=$(run layout show | awk -F': ' '/Current cluster layout version/{print $2; exit}')
	run layout apply --version "$((CURRENT_VERSION + 1))"
else
	echo "Layout ya asignado, se omite."
fi

if ! run bucket list | grep -qE "[[:space:]]${BUCKET}[[:space:]]"; then
	echo "Creando bucket '$BUCKET'..."
	run bucket create "$BUCKET"
else
	echo "Bucket '$BUCKET' ya existe, se omite."
fi

if ! run key list | grep -q "talleros-app"; then
	echo "Importando la access key de la app (desde .env)..."
	run key import "$GARAGE_S3_ACCESS_KEY" "$GARAGE_S3_SECRET_KEY" --yes -n talleros-app
else
	echo "Key 'talleros-app' ya existe, se omite."
fi

run bucket allow --read --write "$BUCKET" --key talleros-app
echo "Garage listo: bucket '$BUCKET' accesible con la access key talleros-app."
