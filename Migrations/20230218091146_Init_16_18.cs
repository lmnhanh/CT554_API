using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554API.Migrations
{
    /// <inheritdoc />
    public partial class Init_16_18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                schema: "dbo",
                table: "Images",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                schema: "dbo",
                table: "Images");
        }
    }
}
