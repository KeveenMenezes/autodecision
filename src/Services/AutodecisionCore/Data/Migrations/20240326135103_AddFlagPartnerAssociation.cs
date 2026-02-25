using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagPartnerAssociation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Triggers
            migrationBuilder.Sql("INSERT INTO `autodecision_core`.`flags` (`id`, `code`, `name`, `description`, `active`, `created_at`, `is_deleted`, `internal_flag`, `is_warning`) VALUES ('45', '251', 'Partner Association', 'Partner Association', '1', '2024-03-26', '0', '0', '0');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Delete Triggers Flags
            migrationBuilder.Sql("DELETE FROM `autodecision_core`.`flags` WHERE (`id` = '45');");
        }
    }
}
