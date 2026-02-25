using Microsoft.EntityFrameworkCore.Migrations;

namespace AutodecisionCore.Data.ManualMigrations
{
    public class CreateDocumentUploadedTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Document uploaded', NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '220', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Document uploaded');");
            migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '253', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Document uploaded');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '220' and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'Document uploaded');");
            migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '253' and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'Document uploaded');");
            migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Document uploaded';");
        }
    }
}