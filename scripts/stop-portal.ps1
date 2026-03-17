$ErrorActionPreference = 'SilentlyContinue'

$connection = Get-NetTCPConnection -LocalPort 5174 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1

if ($connection) {
    Stop-Process -Id $connection.OwningProcess -Force
}
