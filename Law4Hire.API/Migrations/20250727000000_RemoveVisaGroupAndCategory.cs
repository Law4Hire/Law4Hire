using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    public partial class RemoveVisaGroupAndCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop legacy VisaGroup foreign key from VisaTypes
            migrationBuilder.DropForeignKey(
                name: "FK_VisaTypes_VisaGroups_VisaGroupId",
                table: "VisaTypes");

            migrationBuilder.DropIndex(
                name: "IX_VisaTypes_VisaGroupId",
                table: "VisaTypes");

            // Remove legacy columns
            migrationBuilder.DropColumn(
                name: "VisaGroupId",
                table: "VisaTypes");
            migrationBuilder.DropColumn(
                name: "Category",
                table: "VisaTypes");

            // Drop the old VisaGroups table
            migrationBuilder.DropTable(
                name: "VisaGroups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the old VisaGroups table
            migrationBuilder.CreateTable(
                name: "VisaGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaGroups", x => x.Id);
                });

            // Re-add legacy columns on VisaTypes
            migrationBuilder.AddColumn<Guid>(
                name: "VisaGroupId",
                table: "VisaTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "VisaTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VisaTypes_VisaGroupId",
                table: "VisaTypes",
                column: "VisaGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisaTypes_VisaGroups_VisaGroupId",
                table: "VisaTypes",
                column: "VisaGroupId",
                principalTable: "VisaGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}