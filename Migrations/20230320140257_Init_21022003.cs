using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_21022003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VenderId",
                schema: "dbo",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Vender",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vender", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_VenderId",
                schema: "dbo",
                table: "Invoices",
                column: "VenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Vender_VenderId",
                schema: "dbo",
                table: "Invoices",
                column: "VenderId",
                principalSchema: "dbo",
                principalTable: "Vender",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Vender_VenderId",
                schema: "dbo",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "Vender",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_VenderId",
                schema: "dbo",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VenderId",
                schema: "dbo",
                table: "Invoices");
        }
    }
}
