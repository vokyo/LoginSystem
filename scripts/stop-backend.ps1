$ErrorActionPreference = 'SilentlyContinue'

$pidFile = 'C:\Users\user\RiderProjects\ConsoleApp1\runlogs\backend.pid'

if (Test-Path $pidFile) {
    $pid = Get-Content $pidFile | Select-Object -First 1
    if ($pid) {
        Stop-Process -Id $pid -Force
    }

    Remove-Item $pidFile -Force
}
