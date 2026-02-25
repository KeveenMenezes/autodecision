using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InsertTriggerAndTriggerFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Triggers
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Undefined', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('old-autodecision', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Connect with Argyle', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Connect with Atomic', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Connect with Pinwheel', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Process after change Offer', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Disconnect payroll', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Connect Debit Card', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Created Face Id', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Connect with OpenBanking', NOW());");

            // Insert TriggerFlags
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '220', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect with Argyle', 'Connect with Atomic', 'Connect with Pinwheel', 'Disconnect payroll');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '236', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect with Argyle', 'Connect with Atomic', 'Connect with Pinwheel', 'Connect with OpenBanking');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '243', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect with Argyle', 'Connect with Atomic', 'Connect with Pinwheel');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '244', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect with OpenBanking');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '245', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect with Argyle', 'Connect with Atomic', 'Connect with Pinwheel', 'Disconnect payroll');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '241', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect Debit Card');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '230', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Created Face Id');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '246', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect with OpenBanking');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Delete Triggers Flags
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '220';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '230';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '236';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '241';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '243';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '244';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '245';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '246';");

            //Delete Triggers
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Undefined';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'old-autodecision';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Connect with Argyle';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Connect with Atomic';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Connect with Pinwheel';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Process after change Offer';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Disconnect payroll';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Connect Debit Card';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Created Face Id';");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Connect with OpenBanking';");
        }
    }
}
