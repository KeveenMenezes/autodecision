using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InsertingTriggersForDebitCardDisconnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Insert Triggers
			migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Disconnect Debit Card', NOW());");

			// Insert TriggerFlags
			migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '241', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Disconnect Debit Card');");

		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
			//Delete Triggers Flags
			migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '241' and trigger_id = (SELECT t.id from autodecision_core.triggers t WHERE t.description = 'Disconnect Debit Card');");

			//Delete Triggers
			migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Connect Debit Card';");
		}
    }
}
