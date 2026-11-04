param(
    [string]$Destino = ".\backups"
)
$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $Destino | Out-Null
$nombre = "stockcontrol-{0}.dump" -f (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$archivo = Join-Path $Destino $nombre
$base = if ($env:PGDATABASE) { $env:PGDATABASE } else { "stockcontrol" }
& pg_dump --format=custom --file=$archivo $base
if ($LASTEXITCODE -ne 0) { throw "pg_dump falló" }
Get-ChildItem $Destino -Filter "stockcontrol-*.dump" |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-14) } |
    Remove-Item
Write-Output $archivo
