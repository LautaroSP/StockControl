# Backup de Postgres

El script es [backup.ps1](backup.ps1) en Windows y [backup.sh](backup.sh) en el servidor. No guarda la clave: usa las variables de entorno.

```
PGHOST=127.0.0.1
PGPORT=5432
PGUSER=stockcontrol_migracion
PGDATABASE=stockcontrol
PGPASSWORD=...
```

En el compose de desarrollo el usuario es `stockcontrol` y la base no está abierta a internet (`127.0.0.1:5432`). Si Postgres está en otra máquina, la cadena del API lleva `SSL Mode=Require`.

## Programar

En Linux, una vez por día y borrar lo que tenga más de 14 días (el script ya borra):

```
0 3 * * * /opt/stockcontrol/deploy/backup.sh /var/backups/stockcontrol
```

En Windows, el Programador de tareas corre `powershell -File deploy\backup.ps1 -Destino D:\backups\stockcontrol`.

## Restaurar

A una base vacía, no encima de la que está en uso:

```
createdb stockcontrol_restaurada
pg_restore --clean --if-exists -d stockcontrol_restaurada stockcontrol-FECHA.dump
```

La prueba de esta tarjeta restauró un dump en una base descartable y leyó una tabla. El resultado está en `backlog/44-auditoria-informe.md`.
