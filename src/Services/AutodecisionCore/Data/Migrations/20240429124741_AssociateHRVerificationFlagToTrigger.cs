using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssociateHRVerificationFlagToTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '250', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Customer info census update');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '250';");
        }
    }
}
