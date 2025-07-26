using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInterviewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisaType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowJson",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WorkflowJson", table: "Users");
            migrationBuilder.DropColumn(name: "VisaType", table: "Users");
            migrationBuilder.DropColumn(name: "Category", table: "Users");
        }
    }
}
