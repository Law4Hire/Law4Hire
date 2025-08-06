@echo off
REM Law4Hire Test Runner - Simple batch version

echo Law4Hire Test Suite Runner
echo =========================

REM Check command line arguments
set TEST_TYPE=%1
if "%TEST_TYPE%"=="" set TEST_TYPE=all

echo Running %TEST_TYPE% tests...

REM Run based on test type
if "%TEST_TYPE%"=="unit" goto :unit
if "%TEST_TYPE%"=="integration" goto :integration  
if "%TEST_TYPE%"=="ui" goto :ui
if "%TEST_TYPE%"=="all" goto :all

echo Invalid test type: %TEST_TYPE%
echo Valid options: all, unit, integration, ui
exit /b 1

:unit
echo.
echo Running Unit Tests...
echo ==================
dotnet test tests/Law4Hire.UnitTests/Law4Hire.UnitTests.csproj
goto :end

:integration
echo.
echo Running Integration Tests...
echo =========================
dotnet test tests/Law4Hire.IntegrationTests/Law4Hire.IntegrationTests.csproj
goto :end

:ui
echo.
echo Running UI Tests...
echo ================
if not exist node_modules (
    echo Installing npm dependencies...
    npm install
)
npm run test:ui
goto :end

:all
echo.
echo Running All Tests...
echo ==================
echo.
echo 1. Unit Tests
echo -------------
dotnet test tests/Law4Hire.UnitTests/Law4Hire.UnitTests.csproj
if errorlevel 1 echo Unit tests failed

echo.
echo 2. Integration Tests  
echo ------------------
dotnet test tests/Law4Hire.IntegrationTests/Law4Hire.IntegrationTests.csproj
if errorlevel 1 echo Integration tests failed

echo.
echo 3. UI Tests
echo -----------
if not exist node_modules (
    echo Installing npm dependencies...
    npm install
)
npm run test:ui
if errorlevel 1 echo UI tests failed

:end
echo.
echo Test execution completed.

REM Usage examples:
REM run-tests.bat          (runs all tests)
REM run-tests.bat unit     (runs only unit tests)
REM run-tests.bat ui       (runs only UI tests)