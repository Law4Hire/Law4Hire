# Law4Hire Test Runner Script
# This script runs all tests in the proper order and provides comprehensive test reporting

param(
    [string]$TestType = "all",  # all, unit, integration, ui
    [switch]$Coverage = $false,
    [switch]$Watch = $false,
    [switch]$Verbose = $false,
    [switch]$Headful = $false   # Run UI tests with visible browser
)

Write-Host "Law4Hire Test Suite Runner" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

# Check if application is running
function Test-ApplicationRunning {
    Write-Host "Checking if applications are running..." -ForegroundColor Yellow
    
    try {
        $webResponse = Invoke-WebRequest -Uri "http://localhost:5161" -Method GET -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        Write-Host "✓ Web application is running on http://localhost:5161" -ForegroundColor Green
    } catch {
        Write-Host "✗ Web application is not running on http://localhost:5161" -ForegroundColor Red
        Write-Host "Please start the web application with: dotnet run --project Law4Hire.Web --urls http://localhost:5161" -ForegroundColor Yellow
        return $false
    }
    
    try {
        $apiResponse = Invoke-WebRequest -Uri "https://localhost:7280/api/lookups/countries" -Method GET -TimeoutSec 5 -UseBasicParsing -SkipCertificateCheck -ErrorAction Stop
        Write-Host "✓ API is running on https://localhost:7280" -ForegroundColor Green
    } catch {
        Write-Host "✗ API is not running on https://localhost:7280" -ForegroundColor Red
        Write-Host "Please start the API with: dotnet run --project Law4Hire.API" -ForegroundColor Yellow
        return $false
    }
    
    return $true
}

# Run unit tests
function Run-UnitTests {
    Write-Host "`nRunning Unit Tests..." -ForegroundColor Cyan
    Write-Host "===================" -ForegroundColor Cyan
    
    $testCommand = "dotnet test tests/Law4Hire.UnitTests/Law4Hire.UnitTests.csproj"
    
    if ($Coverage) {
        $testCommand += " --collect:'XPlat Code Coverage'"
    }
    
    if ($Verbose) {
        $testCommand += " --verbosity detailed"
    }
    
    Write-Host "Executing: $testCommand" -ForegroundColor Gray
    
    $unitTestResult = Invoke-Expression $testCommand
    $unitTestExitCode = $LASTEXITCODE
    
    if ($unitTestExitCode -eq 0) {
        Write-Host "✓ Unit tests passed" -ForegroundColor Green
    } else {
        Write-Host "✗ Unit tests failed" -ForegroundColor Red
    }
    
    return $unitTestExitCode
}

# Run integration tests  
function Run-IntegrationTests {
    Write-Host "`nRunning Integration Tests..." -ForegroundColor Cyan
    Write-Host "=========================" -ForegroundColor Cyan
    
    $testCommand = "dotnet test tests/Law4Hire.IntegrationTests/Law4Hire.IntegrationTests.csproj"
    
    if ($Coverage) {
        $testCommand += " --collect:'XPlat Code Coverage'"
    }
    
    if ($Verbose) {
        $testCommand += " --verbosity detailed"
    }
    
    Write-Host "Executing: $testCommand" -ForegroundColor Gray
    
    $integrationTestResult = Invoke-Expression $testCommand
    $integrationTestExitCode = $LASTEXITCODE
    
    if ($integrationTestExitCode -eq 0) {
        Write-Host "✓ Integration tests passed" -ForegroundColor Green
    } else {
        Write-Host "✗ Integration tests failed" -ForegroundColor Red
    }
    
    return $integrationTestExitCode
}

# Run UI tests
function Run-UITests {
    Write-Host "`nRunning UI Tests..." -ForegroundColor Cyan
    Write-Host "================" -ForegroundColor Cyan
    
    # Check if npm dependencies are installed
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing npm dependencies..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Failed to install npm dependencies" -ForegroundColor Red
            return 1
        }
    }
    
    $env:NODE_ENV = if ($Headful) { "development" } else { "production" }
    
    $testCommand = "npm run test:ui"
    
    if ($Coverage) {
        $testCommand = "npm run test:ui:coverage"
    }
    
    if ($Watch) {
        $testCommand = "npm run test:ui:watch"
    }
    
    Write-Host "Executing: $testCommand" -ForegroundColor Gray
    Write-Host "Environment: NODE_ENV=$env:NODE_ENV" -ForegroundColor Gray
    
    $uiTestResult = Invoke-Expression $testCommand
    $uiTestExitCode = $LASTEXITCODE
    
    if ($uiTestExitCode -eq 0) {
        Write-Host "✓ UI tests passed" -ForegroundColor Green
    } else {
        Write-Host "✗ UI tests failed" -ForegroundColor Red
    }
    
    return $uiTestExitCode
}

# Main execution
Write-Host "Test Configuration:" -ForegroundColor Yellow
Write-Host "- Test Type: $TestType" -ForegroundColor Gray
Write-Host "- Coverage: $Coverage" -ForegroundColor Gray
Write-Host "- Watch Mode: $Watch" -ForegroundColor Gray
Write-Host "- Verbose: $Verbose" -ForegroundColor Gray
Write-Host "- Headful UI: $Headful" -ForegroundColor Gray

$allExitCodes = @()

# Check if applications are running for UI tests
if ($TestType -eq "all" -or $TestType -eq "ui") {
    if (-not (Test-ApplicationRunning)) {
        Write-Host "Cannot run UI tests without applications running" -ForegroundColor Red
        exit 1
    }
}

try {
    # Run tests based on type
    switch ($TestType.ToLower()) {
        "unit" {
            $allExitCodes += Run-UnitTests
        }
        "integration" {
            $allExitCodes += Run-IntegrationTests
        }
        "ui" {
            $allExitCodes += Run-UITests
        }
        "all" {
            Write-Host "Running all test suites in sequence..." -ForegroundColor Magenta
            
            # Run in order: fastest to slowest
            $allExitCodes += Run-UnitTests
            $allExitCodes += Run-IntegrationTests
            $allExitCodes += Run-UITests
        }
        default {
            Write-Host "Invalid test type: $TestType" -ForegroundColor Red
            Write-Host "Valid options: all, unit, integration, ui" -ForegroundColor Yellow
            exit 1
        }
    }
    
    # Summary
    Write-Host "`nTest Summary" -ForegroundColor Magenta
    Write-Host "============" -ForegroundColor Magenta
    
    $failedTests = $allExitCodes | Where-Object { $_ -ne 0 }
    
    if ($failedTests.Count -eq 0) {
        Write-Host "🎉 All tests passed!" -ForegroundColor Green
        
        if ($Coverage) {
            Write-Host "`nCoverage reports generated in:" -ForegroundColor Yellow
            Write-Host "- .NET: TestResults/*/coverage.cobertura.xml" -ForegroundColor Gray
            Write-Host "- UI: coverage/" -ForegroundColor Gray
        }
        
        exit 0
    } else {
        Write-Host "❌ Some tests failed" -ForegroundColor Red
        Write-Host "Failed test suites: $($failedTests.Count)" -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "Error running tests: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test execution examples:
<#
# Run all tests
.\run-tests.ps1

# Run only unit tests
.\run-tests.ps1 -TestType unit

# Run UI tests with coverage in headful mode
.\run-tests.ps1 -TestType ui -Coverage -Headful

# Run all tests with verbose output and coverage
.\run-tests.ps1 -TestType all -Coverage -Verbose

# Run UI tests in watch mode
.\run-tests.ps1 -TestType ui -Watch
#>