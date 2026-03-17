$ErrorActionPreference = 'Stop'

$root = 'C:\Users\user\RiderProjects\ConsoleApp1'
$logDir = Join-Path $root 'runlogs'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$stdout = Join-Path $logDir 'backend.log'
$stderr = Join-Path $logDir 'backend.err.log'
$pidFile = Join-Path $logDir 'backend.pid'

$dotnet = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'
$args = @(
    'run',
    '--project',
    'src\backend\CheckInSystem.Api\CheckInSystem.Api.csproj',
    '--no-build',
    '--urls',
    'http://localhost:5246'
)

$process = Start-Process -FilePath $dotnet -ArgumentList $args -WorkingDirectory $root -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$process.Id | Set-Content $pidFile
$process.Id
