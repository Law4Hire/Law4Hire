using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewVisaColumnStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to BaseVisaTypes
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "BaseVisaTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "BaseVisaTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BaseVisaTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Question1",
                table: "BaseVisaTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Question2",
                table: "BaseVisaTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Question3",
                table: "BaseVisaTypes",
                type: "nvarchar(max)",
                nullable: true);

            // Create unique index on Code
            migrationBuilder.CreateIndex(
                name: "IX_BaseVisaTypes_Code",
                table: "BaseVisaTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseVisaTypes_Status",
                table: "BaseVisaTypes",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_BaseVisaTypes_Code",
                table: "BaseVisaTypes");

            migrationBuilder.DropIndex(
                name: "IX_BaseVisaTypes_Status",
                table: "BaseVisaTypes");

            // Drop new columns
            migrationBuilder.DropColumn(
                name: "Code",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "Description",
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
        }
    }
}
