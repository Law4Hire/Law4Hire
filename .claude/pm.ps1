# Quick Project Manager - Shorthand script
# Usage examples:
# .\pm.ps1 api start
# .\pm.ps1 web stop  
# .\pm.ps1 scraper restart
# .\pm.ps1 govscraper stop

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$ProjectShort,
    
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateSet("start", "stop", "restart")]
    [string]$Action
)

# Map short names to full project names
$ProjectMap = @{
    "api" = "Law4Hire.API"
    "web" = "Law4Hire.Web"
    "scraper" = "Law4Hire.Scraper"
    "govscraper" = "Law4Hire.GovScraper"
    "gov" = "Law4Hire.GovScraper"
    "mobile" = "Law4Hire.Mobile"
    "maui" = "Law4Hire.Mobile"
}

$FullProjectName = $ProjectMap[$ProjectShort.ToLower()]

if (-not $FullProjectName) {
    Write-Host "Invalid project name. Available options:" -ForegroundColor Red
    $ProjectMap.GetEnumerator() | ForEach-Object { Write-Host "  $($_.Key) -> $($_.Value)" -ForegroundColor Yellow }
    exit 1
}

# Call the main ProjectManager script
$scriptPath = Join-Path $PSScriptRoot "ProjectManager.ps1"
& powershell.exe -ExecutionPolicy Bypass -File $scriptPath -Project $FullProjectName -Action $Action