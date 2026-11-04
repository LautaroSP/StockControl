#!/bin/sh
set -eu
destino="${1:-./backups}"
mkdir -p "$destino"
archivo="$destino/stockcontrol-$(date -u +%Y%m%dT%H%M%SZ).dump"
pg_dump --format=custom --file="$archivo" "${PGDATABASE:-stockcontrol}"
find "$destino" -name 'stockcontrol-*.dump' -mtime +14 -delete
echo "$archivo"
