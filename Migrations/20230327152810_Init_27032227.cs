using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_27032227 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "ManualValue",
                schema: "dbo",
                table: "Stocks",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManualValue",
                schema: "dbo",
                table: "Stocks");
        }
    }
}
