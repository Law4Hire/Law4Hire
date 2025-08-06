using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldVisaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only drop the old columns that are no longer needed
            // The new columns already exist from manual schema changes
            
            migrationBuilder.DropColumn(
                name: "VisaAppropriateFor",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "VisaDescription", 
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "VisaName",
                table: "BaseVisaTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseVisaTypes_CategoryClasses_CategoryClassId",
                table: "BaseVisaTypes");

            migrationBuilder.DropTable(
                name: "CategoryClasses");

            migrationBuilder.DropIndex(
                name: "IX_BaseVisaTypes_CategoryClassId",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "CategoryClassId",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "Question1",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "Question2",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "Question3",
                table: "BaseVisaTypes");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "BaseVisaTypes",
                newName: "VisaDescription");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "BaseVisaTypes",
                newName: "VisaName");

            migrationBuilder.RenameIndex(
                name: "IX_BaseVisaTypes_Code",
                table: "BaseVisaTypes",
                newName: "IX_BaseVisaTypes_VisaName");

            migrationBuilder.AddColumn<string>(
                name: "VisaAppropriateFor",
                table: "BaseVisaTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
