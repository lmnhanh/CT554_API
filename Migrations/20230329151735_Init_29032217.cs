using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_29032217 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Users_UserId",
                schema: "dbo",
                table: "Carts");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "dbo",
                table: "Carts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Users_UserId",
                schema: "dbo",
                table: "Carts",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Users_UserId",
                schema: "dbo",
                table: "Carts");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "dbo",
                table: "Carts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Users_UserId",
                schema: "dbo",
                table: "Carts",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
