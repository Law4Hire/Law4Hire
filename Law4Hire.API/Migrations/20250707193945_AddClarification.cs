using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class AddClarification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuingAgency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalProfessionals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BarNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalProfessionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalProfessionals_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisaTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserVisas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisaTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVisas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVisas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVisas_VisaTypes_VisaTypeId",
                        column: x => x.VisaTypeId,
                        principalTable: "VisaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisaDocumentRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisaTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DocumentTypeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaDocumentRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisaDocumentRequirements_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisaDocumentRequirements_DocumentTypes_DocumentTypeId1",
                        column: x => x.DocumentTypeId1,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisaDocumentRequirements_VisaTypes_VisaTypeId",
                        column: x => x.VisaTypeId,
                        principalTable: "VisaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDocumentStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserVisaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisaTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentTypeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocumentStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDocumentStatuses_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDocumentStatuses_DocumentTypes_DocumentTypeId1",
                        column: x => x.DocumentTypeId1,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserDocumentStatuses_UserVisas_UserVisaId",
                        column: x => x.UserVisaId,
                        principalTable: "UserVisas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDocumentStatuses_VisaTypes_VisaTypeId",
                        column: x => x.VisaTypeId,
                        principalTable: "VisaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentStatuses_DocumentTypeId",
                table: "UserDocumentStatuses",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentStatuses_DocumentTypeId1",
                table: "UserDocumentStatuses",
                column: "DocumentTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentStatuses_UserVisaId",
                table: "UserDocumentStatuses",
                column: "UserVisaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentStatuses_VisaTypeId",
                table: "UserDocumentStatuses",
                column: "VisaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVisas_UserId",
                table: "UserVisas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVisas_VisaTypeId",
                table: "UserVisas",
                column: "VisaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisaDocumentRequirements_DocumentTypeId",
                table: "VisaDocumentRequirements",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisaDocumentRequirements_DocumentTypeId1",
                table: "VisaDocumentRequirements",
                column: "DocumentTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_VisaDocumentRequirements_VisaTypeId",
                table: "VisaDocumentRequirements",
                column: "VisaTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegalProfessionals");

            migrationBuilder.DropTable(
                name: "UserDocumentStatuses");

            migrationBuilder.DropTable(
                name: "VisaDocumentRequirements");

            migrationBuilder.DropTable(
                name: "UserVisas");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "VisaTypes");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Users");
        }
    }
}
