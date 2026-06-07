<#
.SYNOPSIS
Starts every API project in this solution.

.EXAMPLE
.\Start-AllApis.ps1

.EXAMPLE
.\Start-AllApis.ps1 -LaunchProfile http

.EXAMPLE
.\Start-AllApis.ps1 -NoRestore
#>
[CmdletBinding()]
param(
    [ValidateSet("http", "https")]
    [string]$LaunchProfile = "https",

    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$apis = @(
    [pscustomobject]@{
        Name = "IdentityService.API"
        Project = Join-Path $root "IdentityService.API\IdentityService.API.csproj"
        Urls = @{
            http = "http://localhost:5084"
            https = "https://localhost:7011; http://localhost:5084"
        }
    },
    [pscustomobject]@{
        Name = "JobApplicationManager.API"
        Project = Join-Path $root "JobApplicationManager.API\JobApplicationManager.API.csproj"
        Urls = @{
            http = "http://localhost:5014"
            https = "https://localhost:7269; http://localhost:5014"
        }
    }
)

function ConvertTo-PowerShellLiteral {
    param([Parameter(Mandatory)][string]$Value)

    return "'" + ($Value -replace "'", "''") + "'"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "The .NET SDK was not found on PATH. Install the .NET SDK or add dotnet to PATH before running this script."
}

$missingProjects = @($apis | Where-Object { -not (Test-Path -LiteralPath $_.Project) })
if ($missingProjects.Count -gt 0) {
    $missingProjectNames = ($missingProjects | ForEach-Object { $_.Project }) -join ", "
    throw "Could not find project file(s): $missingProjectNames"
}

$terminal = Get-Command pwsh -ErrorAction SilentlyContinue
if (-not $terminal) {
    $terminal = Get-Command powershell.exe -ErrorAction Stop
}

$restoreArgument = if ($NoRestore) { "--no-restore" } else { "" }

foreach ($api in $apis) {
    $projectDir = Split-Path -Parent $api.Project
    $projectDirLiteral = ConvertTo-PowerShellLiteral $projectDir
    $projectLiteral = ConvertTo-PowerShellLiteral $api.Project
    $windowTitleLiteral = ConvertTo-PowerShellLiteral $api.Name
    $apiName = $api.Name
    $apiUrls = $api.Urls[$LaunchProfile]

    $command = @"
`$host.UI.RawUI.WindowTitle = $windowTitleLiteral
Set-Location -LiteralPath $projectDirLiteral
Write-Host "Starting $apiName with launch profile '$LaunchProfile'..." -ForegroundColor Cyan
Write-Host "URLs: $apiUrls" -ForegroundColor DarkCyan
dotnet run --project $projectLiteral --launch-profile $LaunchProfile $restoreArgument
if (`$LASTEXITCODE -ne 0) {
    Write-Host "$apiName stopped with exit code `$LASTEXITCODE." -ForegroundColor Red
}
"@

    $encodedCommand = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))

    Start-Process `
        -FilePath $terminal.Source `
        -ArgumentList @("-NoExit", "-NoProfile", "-ExecutionPolicy", "Bypass", "-EncodedCommand", $encodedCommand) `
        -WorkingDirectory $projectDir

    Start-Sleep -Milliseconds 500
}

Write-Host "Started $($apis.Count) API windows." -ForegroundColor Green
Write-Host "Press Ctrl+C in each API window to stop it."
