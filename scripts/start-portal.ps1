$ErrorActionPreference = 'Stop'

$root = 'C:\Users\user\RiderProjects\ConsoleApp1\src\frontend\checkin-portal'
$logDir = 'C:\Users\user\RiderProjects\ConsoleApp1\runlogs'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$stdout = Join-Path $logDir 'portal.log'
$stderr = Join-Path $logDir 'portal.err.log'

$process = Start-Process cmd.exe -ArgumentList '/c npm run dev -- --host 0.0.0.0 --port 5174' -WorkingDirectory $root -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$process.Id
