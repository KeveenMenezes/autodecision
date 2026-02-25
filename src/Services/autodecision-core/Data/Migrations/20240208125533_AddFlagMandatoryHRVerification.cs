using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagMandatoryHRVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Triggers
            migrationBuilder.Sql("INSERT INTO `autodecision_core`.`flags` (`id`, `code`, `name`, `description`, `active`, `created_at`, `is_deleted`, `internal_flag`, `is_warning`) VALUES ('44', '250', 'Mandatory HR Verification', 'Mandatory HR Verification', '1', '2024-02-08', '0', '0', '0');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Delete Triggers Flags
            migrationBuilder.Sql("DELETE FROM `autodecision_core`.`flags` WHERE (`id` = '44');");
        }
    }
}
