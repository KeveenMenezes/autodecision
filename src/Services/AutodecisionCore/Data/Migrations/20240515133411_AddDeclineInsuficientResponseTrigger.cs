using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeclineInsuficientResponseTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("INSERT INTO autodecision_core.triggers (description, created_at) VALUES ('Decline Insuficient Response', NOW());");
			migrationBuilder.Sql("INSERT INTO autodecision_core.trigger_flags (trigger_id, flag_code, created_at) SELECT t.id, '249', NOW() FROM autodecision_core.triggers t WHERE t.description IN ('Decline Insuficient Response');");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DELETE FROM autodecision_core.trigger_flags WHERE flag_code = '249';");
			migrationBuilder.Sql("DELETE FROM autodecision_core.triggers WHERE description = 'Decline Insuficient Response';");
		}
	}
}
