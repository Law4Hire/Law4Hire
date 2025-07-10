using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class _00000000000002_GenerateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<Guid>(
                name: "VisaGroupId",
                table: "VisaTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "VisaGroups",
                columns: new[] { "Id", "Name", "Description" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 1, null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 2, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 3, null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 4, null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), 5, null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), 6, null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), 7, null }
                });

            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '11111111-1111-1111-1111-111111111111' WHERE Category = 'Visit';");
            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '22222222-2222-2222-2222-222222222222' WHERE Category = 'Immigrate';");
            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '33333333-3333-3333-3333-333333333333' WHERE Category = 'Investment';");
            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '44444444-4444-4444-4444-444444444444' WHERE Category = 'Work';");
            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '55555555-5555-5555-5555-555555555555' WHERE Category = 'Protect';");
            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '66666666-6666-6666-6666-666666666666' WHERE Category = 'Study';");
            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '77777777-7777-7777-7777-777777777777' WHERE Category = 'Family';");

            // Ensure all existing visa types have an assigned group before making the column non-nullable
            migrationBuilder.Sql("UPDATE VisaTypes SET VisaGroupId = '11111111-1111-1111-1111-111111111111' WHERE VisaGroupId IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "VisaGroupId",
                table: "VisaTypes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

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

