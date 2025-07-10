using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class CorrectErrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VisaGroupId",
                table: "VisaTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "VisaGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaGroups", x => x.Id);
                });

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
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisaTypes_VisaGroups_VisaGroupId",
                table: "VisaTypes");

            migrationBuilder.DropTable(
                name: "VisaGroups");

            migrationBuilder.DropIndex(
                name: "IX_VisaTypes_VisaGroupId",
                table: "VisaTypes");

            migrationBuilder.DropColumn(
                name: "VisaGroupId",
                table: "VisaTypes");
        }
    }
}
