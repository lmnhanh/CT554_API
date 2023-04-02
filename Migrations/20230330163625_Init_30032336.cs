using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_30032336 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "RealQuantity",
                schema: "dbo",
                table: "Carts",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealQuantity",
                schema: "dbo",
                table: "Carts");
        }
    }
}
