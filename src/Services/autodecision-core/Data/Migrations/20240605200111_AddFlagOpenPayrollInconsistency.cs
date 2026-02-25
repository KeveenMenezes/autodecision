using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagOpenPayrollInconsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO `autodecision_core`.`flags` (`id`, `code`, `name`, `description`, `active`, `created_at`, `is_deleted`, `internal_flag`, `is_warning`) VALUES ('47', '253', 'Open Payroll Inconsistency', 'Open Payroll Inconsistency', '1', CURRENT_DATE(), '0', '0', '0');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM autodecision_core.flags WHERE id = 47;");
        }
    }
}
