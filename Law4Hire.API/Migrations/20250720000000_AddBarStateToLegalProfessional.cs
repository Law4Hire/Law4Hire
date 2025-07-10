using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBarStateToLegalProfessional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BarState",
                table: "LegalProfessionals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarState",
                table: "LegalProfessionals");
        }
    }
}
