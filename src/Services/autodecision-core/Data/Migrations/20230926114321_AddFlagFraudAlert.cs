using AutodecisionCore.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230926114321_AddFlagFraudAlert")]
    public partial class AddFlagFraudAlert : Migration
    {
        /// <inheritdoc />
    
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Insert Triggers
			migrationBuilder.Sql("INSERT INTO `autodecision_core`.`flags` (`id`, `code`, `name`, `description`, `active`, `created_at`, `is_deleted`, `internal_flag`, `is_warning`) VALUES ('43', '249', 'Fraud Alert', 'Fraud Alert', '1', '2023-09-26', '0', '0', '0');");

		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
			//Delete Triggers Flags
			migrationBuilder.Sql("DELETE FROM `autodecision_core`.`flags` WHERE (`id` = '39');");

		}
    }
}
