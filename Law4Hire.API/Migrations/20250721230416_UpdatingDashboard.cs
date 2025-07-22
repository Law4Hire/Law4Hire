using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GovernmentLink",
                table: "DocumentTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGovernmentProvided",
                table: "DocumentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "DocumentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("3e27b6a3-8a52-4185-854c-5f584aea28e8"),
                columns: new[] { "GovernmentLink", "IsGovernmentProvided", "IsRequired" },
                values: new object[] { null, false, true });

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("678634b3-de95-4f16-83c9-dc86aad68723"),
                columns: new[] { "GovernmentLink", "IsGovernmentProvided", "IsRequired" },
                values: new object[] { null, false, true });

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("fe86ca4b-3808-482b-89fb-e2fc9375684b"),
                columns: new[] { "GovernmentLink", "IsGovernmentProvided", "IsRequired" },
                values: new object[] { null, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GovernmentLink",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "IsGovernmentProvided",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "DocumentTypes");
        }
    }
}
