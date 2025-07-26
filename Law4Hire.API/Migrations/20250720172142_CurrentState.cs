using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class CurrentState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegalProfessionals_AspNetUsers_Id",
                table: "LegalProfessionals");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVisas_AspNetUsers_UserId",
                table: "UserVisas");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVisas_VisaTypes_VisaTypeId",
                table: "UserVisas");

            migrationBuilder.CreateTable(
                name: "VisaInterviewStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentVisaOptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedVisaType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisaWorkflowJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtractedDocumentsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastClientMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastBotMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsReset = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaInterviewStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisaInterviewStates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegalProfessionals_AspNetUsers_Id",
                table: "LegalProfessionals");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDocumentStatuses_AspNetUsers_UserId",
                table: "UserDocumentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVisas_AspNetUsers_UserId",
                table: "UserVisas");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVisas_VisaTypes_VisaTypeId",
                table: "UserVisas");

            migrationBuilder.DropTable(
                name: "ScrapeLogs");

            migrationBuilder.DropTable(
                name: "VisaInterviewStates");

            migrationBuilder.DropTable(
                name: "VisaTypeQuestions");

            migrationBuilder.DropIndex(
                name: "IX_UserDocumentStatuses_UserId",
                table: "UserDocumentStatuses");

            migrationBuilder.DeleteData(
                table: "VisaDocumentRequirements",
                keyColumn: "Id",
                keyValue: new Guid("44e14fd0-418f-4719-8e94-a2fb862faf0f"));

            migrationBuilder.DeleteData(
                table: "VisaDocumentRequirements",
                keyColumn: "Id",
                keyValue: new Guid("51860cfe-ba25-4461-8134-77c8fe6939fe"));

            migrationBuilder.DeleteData(
                table: "VisaDocumentRequirements",
                keyColumn: "Id",
                keyValue: new Guid("e8e5455e-1878-4b73-afca-33a32d4b66ed"));

            migrationBuilder.DeleteData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("3e27b6a3-8a52-4185-854c-5f584aea28e8"));

            migrationBuilder.DeleteData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("678634b3-de95-4f16-83c9-dc86aad68723"));

            migrationBuilder.DeleteData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("fe86ca4b-3808-482b-89fb-e2fc9375684b"));

            migrationBuilder.DeleteData(
                table: "VisaTypes",
                keyColumn: "Id",
                keyValue: new Guid("162e3e30-ec8b-438e-8f96-e836465d0908"));

            migrationBuilder.DropColumn(
                name: "BarState",
                table: "LegalProfessionals");

            migrationBuilder.DropColumn(
                name: "VisaType",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_LegalProfessionals_AspNetUsers_Id",
                table: "LegalProfessionals",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVisas_AspNetUsers_UserId",
                table: "UserVisas",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVisas_VisaTypes_VisaTypeId",
                table: "UserVisas",
                column: "VisaTypeId",
                principalTable: "VisaTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
