using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InsertCreditPolicyFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO autodecision_core.flags (code, name, description, active, created_at, is_deleted, internal_flag, is_warning) VALUES('248', 'Credit Policy is missing', 'Credit Policy is missing', '1', NOW(), '0', '0', '0');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM autodecision_core.flags WHERE code = \"248\";");
        }
    }
}
