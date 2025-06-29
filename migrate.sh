#!/bin/bash
# Migration Commands for Law4Hire (.NET 9)

echo "Law4Hire Entity Framework Migration Commands (.NET 9)"
echo "===================================================="

# Navigate to solution root
cd "$(dirname "$0")"

echo "1. Adding Initial Migration..."
dotnet ef migrations add InitialCreate --project Law4Hire.Infrastructure --startup-project Law4Hire.API --output-dir Data/Migrations

echo "2. Updating Database..."
dotnet ef database update --project Law4Hire.Infrastructure --startup-project Law4Hire.API

echo "3. Migration completed successfully!"

# Optional: Generate SQL script
echo "4. Generating SQL script..."
dotnet ef migrations script --project Law4Hire.Infrastructure --startup-project Law4Hire.API --output database-script.sql

echo "Done! Database is ready for use."
echo ""
echo "Next steps:"
echo "- Update connection strings in appsettings.json"
echo "- Run 'dotnet build' to build the solution"
echo "- Run 'dotnet run --project Law4Hire.API' to start the API"
echo "- Run 'dotnet run --project Law4Hire.Web' to start the web app"
