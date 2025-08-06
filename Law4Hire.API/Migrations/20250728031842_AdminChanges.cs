using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Law4Hire.API.Migrations
{
    /// <inheritdoc />
    public partial class AdminChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VisaTypeId",
                table: "ServicePackages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_VisaTypeId",
                table: "ServicePackages",
                column: "VisaTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePackages_BaseVisaTypes_VisaTypeId",
                table: "ServicePackages",
                column: "VisaTypeId",
                principalTable: "BaseVisaTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePackages_BaseVisaTypes_VisaTypeId",
                table: "ServicePackages");

            migrationBuilder.DropIndex(
                name: "IX_ServicePackages_VisaTypeId",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "VisaTypeId",
                table: "ServicePackages");
        }
    }
}
