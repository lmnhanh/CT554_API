using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_28032122 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Total",
                schema: "dbo",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Total",
                schema: "dbo",
                table: "Orders");
        }
    }
}
