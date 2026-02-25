using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class removefieldsfrommap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_application_flags_application_cores_application_core_id",
                table: "application_flags");

            migrationBuilder.DropForeignKey(
                name: "fk_application_flags_internal_messages_application_flags_applic",
                table: "application_flags_internal_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_application_processes_application_cores_application_core_id",
                table: "application_processes");

            migrationBuilder.DropForeignKey(
                name: "fk_auto_approval_rules_application_cores_application_core_id",
                table: "auto_approval_rules");

            migrationBuilder.AlterColumn<int>(
                name: "application_core_id",
                table: "auto_approval_rules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "application_core_id",
                table: "application_processes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "application_flag_id",
                table: "application_flags_internal_messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "application_core_id",
                table: "application_flags",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "fk_application_flags_application_cores_application_core_id",
                table: "application_flags",
                column: "application_core_id",
                principalTable: "application_cores",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_application_flags_internal_messages_application_flags_applic",
                table: "application_flags_internal_messages",
                column: "application_flag_id",
                principalTable: "application_flags",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_application_processes_application_cores_application_core_id",
                table: "application_processes",
                column: "application_core_id",
                principalTable: "application_cores",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_auto_approval_rules_application_cores_application_core_id",
                table: "auto_approval_rules",
                column: "application_core_id",
                principalTable: "application_cores",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_application_flags_application_cores_application_core_id",
                table: "application_flags");

            migrationBuilder.DropForeignKey(
                name: "fk_application_flags_internal_messages_application_flags_applic",
                table: "application_flags_internal_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_application_processes_application_cores_application_core_id",
                table: "application_processes");

            migrationBuilder.DropForeignKey(
                name: "fk_auto_approval_rules_application_cores_application_core_id",
                table: "auto_approval_rules");

            migrationBuilder.AlterColumn<int>(
                name: "application_core_id",
                table: "auto_approval_rules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "application_core_id",
                table: "application_processes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "application_flag_id",
                table: "application_flags_internal_messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "application_core_id",
                table: "application_flags",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_application_flags_application_cores_application_core_id",
                table: "application_flags",
                column: "application_core_id",
                principalTable: "application_cores",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_application_flags_internal_messages_application_flags_applic",
                table: "application_flags_internal_messages",
                column: "application_flag_id",
                principalTable: "application_flags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_application_processes_application_cores_application_core_id",
                table: "application_processes",
                column: "application_core_id",
                principalTable: "application_cores",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_auto_approval_rules_application_cores_application_core_id",
                table: "auto_approval_rules",
                column: "application_core_id",
                principalTable: "application_cores",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
