using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllFlagsApprovedTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Triggers
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('All flags approved', NOW());");

            // Insert TriggerFlags
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '180', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('All flags approved');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '209', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('All flags approved');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Delete Triggers Flags
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code in ('180', '209') and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'All flags approved');");

            //Delete Triggers
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'All flags approved';");
        }
    }
}