using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class CorrectErrors2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "Id", "Description", "FormNumber", "IssuingAgency", "Name" },
                values: new object[,]
                {
                    { new Guid("3e27b6a3-8a52-4185-854c-5f584aea28e8"), "For H-1B and other workers", "I-129", "USCIS", "Petition for a Nonimmigrant Worker" },
                    { new Guid("678634b3-de95-4f16-83c9-dc86aad68723"), "Application for temporary visas", "DS-160", "DOS", "Online Nonimmigrant Visa Application" },
                    { new Guid("fe86ca4b-3808-482b-89fb-e2fc9375684b"), "Sponsor financial support form", "I-864", "USCIS", "Affidavit of Support" }
                });

            migrationBuilder.InsertData(
                table: "VisaTypes",
                columns: new[] { "Id", "Category", "Description", "Name", "VisaGroupId" },
                values: new object[] { new Guid("162e3e30-ec8b-438e-8f96-e836465d0908"), "Work", "Work visa for specialty occupations", "H1B Specialty Occupation", new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.InsertData(
                table: "VisaDocumentRequirements",
                columns: new[] { "Id", "DocumentTypeId", "IsRequired", "VisaTypeId" },
                values: new object[,]
                {
                    { new Guid("44e14fd0-418f-4719-8e94-a2fb862faf0f"), new Guid("678634b3-de95-4f16-83c9-dc86aad68723"), true, new Guid("162e3e30-ec8b-438e-8f96-e836465d0908") },
                    { new Guid("51860cfe-ba25-4461-8134-77c8fe6939fe"), new Guid("3e27b6a3-8a52-4185-854c-5f584aea28e8"), true, new Guid("162e3e30-ec8b-438e-8f96-e836465d0908") },
                    { new Guid("e8e5455e-1878-4b73-afca-33a32d4b66ed"), new Guid("fe86ca4b-3808-482b-89fb-e2fc9375684b"), true, new Guid("162e3e30-ec8b-438e-8f96-e836465d0908") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
