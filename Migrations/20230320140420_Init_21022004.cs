using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_21022004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Vender_VenderId",
                schema: "dbo",
                table: "Invoices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vender",
                schema: "dbo",
                table: "Vender");

            migrationBuilder.RenameTable(
                name: "Vender",
                schema: "dbo",
                newName: "Venders",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Venders",
                schema: "dbo",
                table: "Venders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Venders_VenderId",
                schema: "dbo",
                table: "Invoices",
                column: "VenderId",
                principalSchema: "dbo",
                principalTable: "Venders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Venders_VenderId",
                schema: "dbo",
                table: "Invoices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Venders",
                schema: "dbo",
                table: "Venders");

            migrationBuilder.RenameTable(
                name: "Venders",
                schema: "dbo",
                newName: "Vender",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vender",
                schema: "dbo",
                table: "Vender",
                column: "Id");

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
    }
}
