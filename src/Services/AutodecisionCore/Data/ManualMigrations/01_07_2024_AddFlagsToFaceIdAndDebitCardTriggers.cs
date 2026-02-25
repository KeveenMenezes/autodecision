using Microsoft.EntityFrameworkCore.Migrations;

namespace AutodecisionCore.Data.ManualMigrations
{
    public class AddFlagsToFaceIdAndDebitCardTriggers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '220', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect Debit Card');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '220', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Created Face Id');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '253', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Connect Debit Card');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '253', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Created Face Id');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '220' and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'Connect Debit Card');");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '220' and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'Created Face Id');");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '253' and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'Connect Debit Card');");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '253' and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'Created Face Id');");
        }
    }
}