using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVisaCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisaSubCategories_VisaCategory_CategoryId",
                table: "VisaSubCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VisaCategory",
                table: "VisaCategory");

            migrationBuilder.RenameTable(
                name: "VisaCategory",
                newName: "VisaCategories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VisaCategories",
                table: "VisaCategories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BaseVisaTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedSubCategories = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    DiscoveredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(3,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseVisaTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseVisaTypes_VisaCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "VisaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseVisaTypes_CategoryId_Name",
                table: "BaseVisaTypes",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseVisaTypes_Status",
                table: "BaseVisaTypes",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_VisaSubCategories_VisaCategories_CategoryId",
                table: "VisaSubCategories",
                column: "CategoryId",
                principalTable: "VisaCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisaSubCategories_VisaCategories_CategoryId",
                table: "VisaSubCategories");

            migrationBuilder.DropTable(
                name: "BaseVisaTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VisaCategories",
                table: "VisaCategories");

            migrationBuilder.RenameTable(
                name: "VisaCategories",
                newName: "VisaCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VisaCategory",
                table: "VisaCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VisaSubCategories_VisaCategory_CategoryId",
                table: "VisaSubCategories",
                column: "CategoryId",
                principalTable: "VisaCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
