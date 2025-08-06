#!/usr/bin/env pwsh
# Law4Hire Project Manager - Automation script for managing project processes
# Usage: .\ProjectManager.ps1 -Project <ProjectName> -Action <start|stop|restart>
# Projects: Law4Hire.API, Law4Hire.Web, Law4Hire.GovScraper, Law4Hire.Scraper, Law4Hire.Mobile

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Law4Hire.API", "Law4Hire.Web", "Law4Hire.GovScraper", "Law4Hire.Scraper", "Law4Hire.Mobile")]
    [string]$Project,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "stop", "restart")]
    [string]$Action
)

# Project configurations
$ProjectConfigs = @{
    "Law4Hire.API" = @{
        Path = "Law4Hire.API"
        ProcessName = "Law4Hire.API"
        Description = "Law4Hire API Server"
    }
    "Law4Hire.Web" = @{
        Path = "Law4Hire.Web"
        ProcessName = "Law4Hire.Web"
        Description = "Law4Hire Web Application"
    }
    "Law4Hire.GovScraper" = @{
        Path = "Law4Hire.GovScraper"
        ProcessName = "Law4Hire.GovScraper"
        Description = "Government Visa Wizard Scraper"
    }
    "Law4Hire.Scraper" = @{
        Path = "Law4Hire.Scraper"
        ProcessName = "Law4Hire.Scraper"
        Description = "Visa Data Scraper"
    }
    "Law4Hire.Mobile" = @{
        Path = "Law4Hire.Mobile"
        ProcessName = "Law4Hire.Mobile"
        Description = "MAUI Mobile Application"
    }
}

$Config = $ProjectConfigs[$Project]
$BaseDir = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $BaseDir $Config.Path

function Write-ProjectLog {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch($Level) {
        "INFO" { "Green" }
        "WARN" { "Yellow" }
        "ERROR" { "Red" }
        default { "White" }
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

function Stop-ProjectProcess {
    param([string]$ProcessName)
    
    Write-ProjectLog "Stopping $ProcessName processes..." "INFO"
    
    $processes = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($processes) {
        foreach ($process in $processes) {
            Write-ProjectLog "Killing process $($process.Name) (PID: $($process.Id))" "WARN"
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        }
        
        # Wait a moment for processes to terminate
        Start-Sleep -Seconds 2
        
        # Verify processes are stopped
        $remainingProcesses = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
        if ($remainingProcesses) {
            Write-ProjectLog "Warning: Some processes may still be running" "WARN"
        } else {
            Write-ProjectLog "All $ProcessName processes stopped successfully" "INFO"
        }
    } else {
        Write-ProjectLog "No $ProcessName processes found running" "INFO"
    }
}

function Start-ProjectProcess {
    param([string]$ProjectPath, [string]$ProjectName)
    
    Write-ProjectLog "Starting $ProjectName..." "INFO"
    
    if (!(Test-Path $ProjectPath)) {
        Write-ProjectLog "Project path not found: $ProjectPath" "ERROR"
        return $false
    }
    
    Set-Location $ProjectPath
    Write-ProjectLog "Changed directory to: $ProjectPath" "INFO"
    
    # Start the project in a new process
    $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory $ProjectPath -PassThru -WindowStyle Normal
    
    if ($process) {
        Write-ProjectLog "$ProjectName started successfully (PID: $($process.Id))" "INFO"
        Write-ProjectLog "You can monitor the process with: Get-Process -Id $($process.Id)" "INFO"
        return $true
    } else {
        Write-ProjectLog "Failed to start $ProjectName" "ERROR"
        return $false
    }
}

function Test-ProjectBuild {
    param([string]$ProjectPath, [string]$ProjectName)
    
    Write-ProjectLog "Testing build for $ProjectName..." "INFO"
    
    Set-Location $ProjectPath
    $buildResult = & dotnet build 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-ProjectLog "$ProjectName build successful" "INFO"
        return $true
    } else {
        Write-ProjectLog "$ProjectName build failed:" "ERROR"
        Write-ProjectLog $buildResult "ERROR"
        return $false
    }
}

# Main execution
Write-ProjectLog "=== Law4Hire Project Manager ===" "INFO"
Write-ProjectLog "Project: $($Config.Description)" "INFO"
Write-ProjectLog "Action: $Action" "INFO"
Write-ProjectLog "Path: $ProjectPath" "INFO"

try {
    switch ($Action) {
        "stop" {
            Stop-ProjectProcess -ProcessName $Config.ProcessName
        }
        
        "start" {
            # First check if any processes are running
            $runningProcesses = Get-Process -Name $Config.ProcessName -ErrorAction SilentlyContinue
            if ($runningProcesses) {
                Write-ProjectLog "$($Config.ProcessName) is already running. Use 'restart' to stop and start." "WARN"
                foreach ($proc in $runningProcesses) {
                    Write-ProjectLog "Running: $($proc.Name) (PID: $($proc.Id))" "WARN"
                }
                exit 1
            }
            
            # Test build first
            if (!(Test-ProjectBuild -ProjectPath $ProjectPath -ProjectName $Project)) {
                Write-ProjectLog "Build failed. Cannot start $Project" "ERROR"
                exit 1
            }
            
            # Start the project
            if (!(Start-ProjectProcess -ProjectPath $ProjectPath -ProjectName $Project)) {
                exit 1
            }
        }
        
        "restart" {
            Write-ProjectLog "Restarting $Project..." "INFO"
            
            # Stop existing processes
            Stop-ProjectProcess -ProcessName $Config.ProcessName
            
            # Wait a moment
            Start-Sleep -Seconds 3
            
            # Test build
            if (!(Test-ProjectBuild -ProjectPath $ProjectPath -ProjectName $Project)) {
                Write-ProjectLog "Build failed. Cannot restart $Project" "ERROR"
                exit 1
            }
            
            # Start the project
            if (!(Start-ProjectProcess -ProjectPath $ProjectPath -ProjectName $Project)) {
                exit 1
            }
        }
    }
    
    Write-ProjectLog "Operation completed successfully" "INFO"
    
} catch {
    Write-ProjectLog "Error: $($_.Exception.Message)" "ERROR"
    exit 1
}