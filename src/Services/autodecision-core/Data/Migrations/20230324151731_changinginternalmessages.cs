using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class changinginternalmessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "application_flags_internal_messages",
                newName: "message_type_id");

            migrationBuilder.AddColumn<int>(
                name: "code",
                table: "application_flags_internal_messages",
                type: "int",
                maxLength: 5,
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "application_flags_internal_messages");

            migrationBuilder.RenameColumn(
                name: "message_type_id",
                table: "application_flags_internal_messages",
                newName: "status");
        }
    }
}
