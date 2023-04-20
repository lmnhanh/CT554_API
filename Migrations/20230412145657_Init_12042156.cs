using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_12042156 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Venders_VenderId",
                schema: "dbo",
                table: "Invoices");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Venders_VenderId",
                schema: "dbo",
                table: "Invoices",
                column: "VenderId",
                principalSchema: "dbo",
                principalTable: "Venders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Venders_VenderId",
                schema: "dbo",
                table: "Invoices");

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
    }
}
