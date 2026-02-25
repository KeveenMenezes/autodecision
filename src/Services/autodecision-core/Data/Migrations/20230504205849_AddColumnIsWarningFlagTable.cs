using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnIsWarningFlagTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_warning",
                table: "application_flags");

            migrationBuilder.AddColumn<bool>(
                name: "is_warning",
                table: "flags",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_warning",
                table: "flags");

            migrationBuilder.AddColumn<bool>(
                name: "is_warning",
                table: "application_flags",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
