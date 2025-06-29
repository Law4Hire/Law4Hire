# Navigate to the solution root
cd "C:\programming\Law4Hire" # Adjust this path if your solution is elsewhere

Write-Host "Cleaning and restoring NuGet packages..." -ForegroundColor Yellow
dotnet clean Law4Hire.sln
dotnet restore Law4Hire.sln

Write-Host "Ensuring project references are correctly set..." -ForegroundColor Green

# Infrastructure references Core
dotnet add "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" reference "Law4Hire.Core\Law4Hire.Core.csproj" | Out-Null
Write-Host "  Law4Hire.Infrastructure -> Law4Hire.Core reference added." -ForegroundColor Cyan

# Application references Core and Infrastructure
dotnet add "Law4Hire.Application\Law4Hire.Application.csproj" reference "Law4Hire.Core\Law4Hire.Core.csproj" | Out-Null
dotnet add "Law4Hire.Application\Law4Hire.Application.csproj" reference "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" | Out-Null
Write-Host "  Law4Hire.Application -> Law4Hire.Core and Law4Hire.Infrastructure references added." -ForegroundColor Cyan

# API references all layers and Shared
dotnet add "Law4Hire.API\Law4Hire.API.csproj" reference "Law4Hire.Core\Law4Hire.Core.csproj" | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" reference "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" reference "Law4Hire.Application\Law4Hire.Application.csproj" | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" reference "Law4Hire.Shared\Law4Hire.Shared.csproj" | Out-Null
Write-Host "  Law4Hire.API -> Core, Infrastructure, Application, Shared references added." -ForegroundColor Cyan

# Web references Core, Application and Shared
dotnet add "Law4Hire.Web\Law4Hire.Web.csproj" reference "Law4Hire.Core\Law4Hire.Core.csproj" | Out-Null
dotnet add "Law4Hire.Web\Law4Hire.Web.csproj" reference "Law4Hire.Application\Law4Hire.Application.csproj" | Out-Null
dotnet add "Law4Hire.Web\Law4Hire.Web.csproj" reference "Law4Hire.Shared\Law4Hire.Shared.csproj" | Out-Null
Write-Host "  Law4Hire.Web -> Core, Application, Shared references added." -ForegroundColor Cyan

# Mobile references Core, Application and Shared
dotnet add "Law4Hire.Mobile\Law4Hire.Mobile.csproj" reference "Law4Hire.Core\Law4Hire.Core.csproj" | Out-Null
dotnet add "Law4Hire.Mobile\Law4Hire.Mobile.csproj" reference "Law4Hire.Application\Law4Hire.Application.csproj" | Out-Null
dotnet add "Law4Hire.Mobile\Law4Hire.Mobile.csproj" reference "Law4Hire.Shared\Law4Hire.Shared.csproj" | Out-Null
Write-Host "  Law4Hire.Mobile -> Core, Application, Shared references added." -ForegroundColor Cyan


Write-Host "Ensuring NuGet packages are correctly installed..." -ForegroundColor Green

# Infrastructure packages
dotnet add "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0 | Out-Null
dotnet add "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Tools --version 9.0.0 | Out-Null
dotnet add "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design --version 9.0.0 | Out-Null
dotnet add "Law4Hire.Infrastructure\Law4Hire.Infrastructure.csproj" package Microsoft.Extensions.Configuration --version 9.0.0 | Out-Null
Write-Host "  Infrastructure packages installed." -ForegroundColor Cyan

# API packages
dotnet add "Law4Hire.API\Law4Hire.API.csproj" package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0 | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" package Microsoft.AspNetCore.SignalR --version 9.0.0 | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0 | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.0 | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" package Microsoft.AspNetCore.OpenApi --version 9.0.0 | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" package Swashbuckle.AspNetCore --version 7.2.0 | Out-Null
dotnet add "Law4Hire.API\Law4Hire.API.csproj" package Microsoft.Extensions.Caching.StackExchangeRedis --version 9.0.0 | Out-Null
Write-Host "  API packages installed." -ForegroundColor Cyan

# Application packages
dotnet add "Law4Hire.Application\Law4Hire.Application.csproj" package FluentValidation --version 11.11.0 | Out-Null
dotnet add "Law4Hire.Application\Law4Hire.Application.csproj" package AutoMapper --version 13.0.1 | Out-Null
dotnet add "Law4Hire.Application\Law4Hire.Application.csproj" package MediatR --version 12.4.1 | Out-Null
Write-Host "  Application packages installed." -ForegroundColor Cyan

# Web packages
dotnet add "Law4Hire.Web\Law4Hire.Web.csproj" package Microsoft.AspNetCore.SignalR.Client --version 9.0.0 | Out-Null
dotnet add "Law4Hire.Web\Law4Hire.Web.csproj" package Microsoft.Extensions.Http --version 9.0.0 | Out-Null
Write-Host "  Web packages installed." -ForegroundColor Cyan

# Shared packages
dotnet add "Law4Hire.Shared\Law4Hire.Shared.csproj" package Microsoft.Extensions.Localization --version 9.0.0 | Out-Null
Write-Host "  Shared packages installed." -ForegroundColor Cyan

# Test packages
dotnet add "tests\Law4Hire.UnitTests\Law4Hire.UnitTests.csproj" package Microsoft.NET.Test.Sdk --version 17.12.0 | Out-Null
dotnet add "tests\Law4Hire.UnitTests\Law4Hire.UnitTests.csproj" package xunit --version 2.9.2 | Out-Null
dotnet add "tests\Law4Hire.UnitTests\Law4Hire.UnitTests.csproj" package xunit.runner.visualstudio --version 2.8.2 | Out-Null
dotnet add "tests\Law4Hire.UnitTests\Law4Hire.UnitTests.csproj" package Moq --version 4.20.72 | Out-Null
dotnet add "tests\Law4Hire.UnitTests\Law4Hire.UnitTests.csproj" package FluentAssertions --version 7.0.0 | Out-Null
dotnet add "tests\Law4Hire.UnitTests\Law4Hire.UnitTests.csproj" package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0 | Out-Null
dotnet add "tests\Law4Hire.IntegrationTests\Law4Hire.IntegrationTests.csproj" package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0 | Out-Null
dotnet add "tests\Law4Hire.IntegrationTests\Law4Hire.IntegrationTests.csproj" package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0 | Out-Null
Write-Host "  Test packages installed." -ForegroundColor Cyan

Write-Host "Attempting full solution build..." -ForegroundColor Green
dotnet build Law4Hire.sln

Write-Host "`nAll references and packages should now be correctly configured. Please review the build output above for any remaining errors." -ForegroundColor Green