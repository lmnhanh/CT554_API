using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CT554_API.Migrations
{
    /// <inheritdoc />
    public partial class Init_10040046 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPartner",
                schema: "dbo",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAsPartner",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAsPartner",
                schema: "dbo",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsPartner",
                schema: "dbo",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
