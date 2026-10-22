# Escucha en 127.0.0.1 y reenvia solo GET /mercadopago/vuelta a la API.
# No escribe la query: ahi viaja el codigo de Mercado Pago.
$ErrorActionPreference = 'Stop'

$proxy = 'http://127.0.0.1:7092/'
$api = 'http://127.0.0.1:5263'
$ruta = '/mercadopago/vuelta'

$escucha = [System.Net.HttpListener]::new()
$escucha.Prefixes.Add($proxy)
$escucha.Start()
Write-Output "Proxy en $proxy"

try {
    while ($escucha.IsListening) {
        $ctx = $escucha.GetContext()
        try {
            $peticion = $ctx.Request
            $respuesta = $ctx.Response
            $camino = $peticion.Url.AbsolutePath
            $query = $peticion.Url.Query
            if ($peticion.HttpMethod -ne 'GET' -or $camino -ne $ruta -or $query.Length -gt 4000) {
                $respuesta.StatusCode = 404
                $respuesta.ContentLength64 = 0
                $respuesta.Close()
                continue
            }
            $ida = [System.Net.HttpWebRequest]::Create($api + $ruta + $query)
            $ida.Method = 'GET'
            $ida.AllowAutoRedirect = $false
            $ida.Timeout = 20000
            $vuelta = $null
            try {
                $vuelta = $ida.GetResponse()
            } catch [System.Net.WebException] {
                $vuelta = $_.Exception.Response
                if (-not $vuelta) { throw }
            }
            $codigo = [int]$vuelta.StatusCode
            $lugar = $vuelta.Headers['Location']
            $tipo = $vuelta.ContentType
            $respuesta.StatusCode = $codigo
            if ($lugar) { $respuesta.Headers['Location'] = $lugar }
            $origen = $vuelta.GetResponseStream()
            $memoria = New-Object System.IO.MemoryStream
            $origen.CopyTo($memoria)
            $bytes = $memoria.ToArray()
            $vuelta.Close()
            if ($bytes.Length -gt 0) {
                if ($tipo) { $respuesta.ContentType = $tipo }
                $respuesta.ContentLength64 = $bytes.Length
                $respuesta.OutputStream.Write($bytes, 0, $bytes.Length)
            }
            $respuesta.Close()
            Write-Output ("vuelta " + $codigo)
        } catch {
            try { $ctx.Response.Abort() } catch {}
            Write-Output "vuelta error"
        }
    }
} finally {
    if ($escucha.IsListening) { $escucha.Stop() }
}
