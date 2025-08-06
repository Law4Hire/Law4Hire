using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaForVisaTypeRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseVisaTypes_VisaCategories_CategoryId",
                table: "BaseVisaTypes");

            migrationBuilder.DropIndex(
                name: "IX_BaseVisaTypes_CategoryId_Name",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "RelatedSubCategories",
                table: "BaseVisaTypes");

            migrationBuilder.RenameColumn(
                name: "LastConfirmedAt",
                table: "BaseVisaTypes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "DiscoveredAt",
                table: "BaseVisaTypes",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "VisaAppropriateFor",
                table: "BaseVisaTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisaDescription",
                table: "BaseVisaTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisaName",
                table: "BaseVisaTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CitizenshipCountryId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaritalStatus",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoryVisaTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisaTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryVisaTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryVisaTypes_BaseVisaTypes_VisaTypeId",
                        column: x => x.VisaTypeId,
                        principalTable: "BaseVisaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryVisaTypes_VisaCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "VisaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CountryCode2 = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IsUNRecognized = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StateCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IsState = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CitizenshipCountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HasRelativesInUS = table.Column<bool>(type: "bit", nullable: true),
                    HasJobOffer = table.Column<bool>(type: "bit", nullable: true),
                    EducationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FearOfPersecution = table.Column<bool>(type: "bit", nullable: true),
                    HasPastVisaDenials = table.Column<bool>(type: "bit", nullable: true),
                    HasStatusViolations = table.Column<bool>(type: "bit", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Countries_CitizenshipCountryId",
                        column: x => x.CitizenshipCountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseVisaTypes_VisaName",
                table: "BaseVisaTypes",
                column: "VisaName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CitizenshipCountryId",
                table: "AspNetUsers",
                column: "CitizenshipCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryVisaTypes_CategoryId_VisaTypeId",
                table: "CategoryVisaTypes",
                columns: new[] { "CategoryId", "VisaTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryVisaTypes_VisaTypeId",
                table: "CategoryVisaTypes",
                column: "VisaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryCode",
                table: "Countries",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryCode2",
                table: "Countries",
                column: "CountryCode2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_CitizenshipCountryId",
                table: "UserProfiles",
                column: "CitizenshipCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USStates_Name",
                table: "USStates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USStates_StateCode",
                table: "USStates",
                column: "StateCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Countries_CitizenshipCountryId",
                table: "AspNetUsers",
                column: "CitizenshipCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Countries_CitizenshipCountryId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CategoryVisaTypes");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "USStates");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_BaseVisaTypes_VisaName",
                table: "BaseVisaTypes");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CitizenshipCountryId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VisaAppropriateFor",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "VisaDescription",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "VisaName",
                table: "BaseVisaTypes");

            migrationBuilder.DropColumn(
                name: "CitizenshipCountryId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "BaseVisaTypes",
                newName: "LastConfirmedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "BaseVisaTypes",
                newName: "DiscoveredAt");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "BaseVisaTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "BaseVisaTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedSubCategories",
                table: "BaseVisaTypes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseVisaTypes_CategoryId_Name",
                table: "BaseVisaTypes",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseVisaTypes_VisaCategories_CategoryId",
                table: "BaseVisaTypes",
                column: "CategoryId",
                principalTable: "VisaCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
