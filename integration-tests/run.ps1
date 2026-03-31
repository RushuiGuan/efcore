#!/usr/bin/env pwsh

param(
    [ValidateSet("None", "Normal", "Detailed", "Diagnostic")]
    [string]$Output = "Detailed"
)

if (-not (Get-Module -ListAvailable -Name Pester)) {
    Write-Host "Installing Pester module..." -ForegroundColor Cyan
    Install-Module -Name Pester -Force -Scope CurrentUser -SkipPublisherCheck
}
Import-Module Pester

$config = New-PesterConfiguration
$config.Run.Path = @(
    "$PSScriptRoot/test-postgres.ps1",
    "$PSScriptRoot/test-sqlserver.ps1"
)
$config.Output.Verbosity = $Output

Invoke-Pester -Configuration $config
