using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedVisaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "VisaTypes",
                columns: new[] { "Id", "Name", "Description", "Category" },
                values: new object[,]
                {
                    { new Guid("1aa2bf5e-7242-49c2-9303-04ddef44e8b1"), "B1/B2 Visitor Visa", "Temporary visit for business or tourism", "Visit" },
                    { new Guid("a8e01e04-27a1-4380-b9cc-cace62830fab"), "Family Based Green Card", "Immigrate through qualifying family", "Immigrate" },
                    { new Guid("cdeb31d4-d778-495b-a4e3-dcd67e1aa737"), "F1 Student Visa", "Academic study in the U.S.", "Study" },
                    { new Guid("162e3e30-ec8b-438e-8f96-e836465d0908"), "H1B Specialty Occupation", "Work visa for specialty occupations", "Work" },
                    { new Guid("a767bd9a-272e-440d-99fc-955e1b1d9303"), "Asylum", "Protection for those fearing persecution", "Protect" }
                });

            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "Id", "FormNumber", "Name", "Description", "IssuingAgency" },
                values: new object[,]
                {
                    { new Guid("2919140a-6230-42a7-a7c8-6972b8a19311"), "I-130", "Petition for Alien Relative", "Establish qualifying family relationship", "USCIS" },
                    { new Guid("411dfd9e-6180-49ab-80b8-3756541de505"), "I-485", "Application to Register Permanent Residence", "Apply for permanent residence", "USCIS" },
                    { new Guid("fe86ca4b-3808-482b-89fb-e2fc9375684b"), "I-864", "Affidavit of Support", "Sponsor financial support form", "USCIS" },
                    { new Guid("678634b3-de95-4f16-83c9-dc86aad68723"), "DS-160", "Online Nonimmigrant Visa Application", "Application for temporary visas", "DOS" },
                    { new Guid("3e27b6a3-8a52-4185-854c-5f584aea28e8"), "I-129", "Petition for a Nonimmigrant Worker", "For H-1B and other workers", "USCIS" }
                });

            migrationBuilder.InsertData(
                table: "VisaDocumentRequirements",
                columns: new[] { "Id", "VisaTypeId", "DocumentTypeId", "IsRequired" },
                values: new object[,]
                {
                    { new Guid("77ff7722-759d-4906-8f2b-d3e393ed9587"), new Guid("1aa2bf5e-7242-49c2-9303-04ddef44e8b1"), new Guid("678634b3-de95-4f16-83c9-dc86aad68723"), true },
                    { new Guid("3b7bb132-1a0c-4d20-9c31-f4d5fd31bf6a"), new Guid("a8e01e04-27a1-4380-b9cc-cace62830fab"), new Guid("2919140a-6230-42a7-a7c8-6972b8a19311"), true },
                    { new Guid("a33e806d-6d34-4f2b-b1b9-0ceaaf479faf"), new Guid("a8e01e04-27a1-4380-b9cc-cace62830fab"), new Guid("411dfd9e-6180-49ab-80b8-3756541de505"), true },
                    { new Guid("abb2b7e9-0fb2-4f56-b96e-a1806f6df7f8"), new Guid("a8e01e04-27a1-4380-b9cc-cace62830fab"), new Guid("fe86ca4b-3808-482b-89fb-e2fc9375684b"), true },
                    { new Guid("c9ea5edd-974b-4150-bee5-2cc506cf9ac9"), new Guid("cdeb31d4-d778-495b-a4e3-dcd67e1aa737"), new Guid("678634b3-de95-4f16-83c9-dc86aad68723"), true },
                    { new Guid("25d18774-32bd-466f-9b6e-677d23dbe0de"), new Guid("162e3e30-ec8b-438e-8f96-e836465d0908"), new Guid("3e27b6a3-8a52-4185-854c-5f584aea28e8"), true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "VisaDocumentRequirements",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("77ff7722-759d-4906-8f2b-d3e393ed9587"),
                    new Guid("3b7bb132-1a0c-4d20-9c31-f4d5fd31bf6a"),
                    new Guid("a33e806d-6d34-4f2b-b1b9-0ceaaf479faf"),
                    new Guid("abb2b7e9-0fb2-4f56-b96e-a1806f6df7f8"),
                    new Guid("c9ea5edd-974b-4150-bee5-2cc506cf9ac9"),
                    new Guid("25d18774-32bd-466f-9b6e-677d23dbe0de")
                });

            migrationBuilder.DeleteData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("2919140a-6230-42a7-a7c8-6972b8a19311"),
                    new Guid("411dfd9e-6180-49ab-80b8-3756541de505"),
                    new Guid("fe86ca4b-3808-482b-89fb-e2fc9375684b"),
                    new Guid("678634b3-de95-4f16-83c9-dc86aad68723"),
                    new Guid("3e27b6a3-8a52-4185-854c-5f584aea28e8")
                });

            migrationBuilder.DeleteData(
                table: "VisaTypes",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("1aa2bf5e-7242-49c2-9303-04ddef44e8b1"),
                    new Guid("a8e01e04-27a1-4380-b9cc-cace62830fab"),
                    new Guid("cdeb31d4-d778-495b-a4e3-dcd67e1aa737"),
                    new Guid("162e3e30-ec8b-438e-8f96-e836465d0908"),
                    new Guid("a767bd9a-272e-440d-99fc-955e1b1d9303")
                });
        }
    }
}
