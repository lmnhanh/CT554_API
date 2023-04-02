using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_02040033 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateProccessed",
                schema: "dbo",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSuccess",
                schema: "dbo",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateProccessed",
                schema: "dbo",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DateSuccess",
                schema: "dbo",
                table: "Orders");
        }
    }
}
