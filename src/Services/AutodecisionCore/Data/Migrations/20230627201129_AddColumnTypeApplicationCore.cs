using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnTypeApplicationCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "application_cores",
                type: "varchar(1)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "application_cores");
        }
    }
}
