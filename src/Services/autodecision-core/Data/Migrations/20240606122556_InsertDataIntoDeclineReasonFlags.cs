using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InsertDataIntoDeclineReasonFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO autodecision_core.decline_reason_flags (flag_code, reason_id, created_at) VALUES (21, 29, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.decline_reason_flags (flag_code, reason_id, created_at) VALUES (194, 13, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.decline_reason_flags (flag_code, reason_id, created_at) VALUES (220, 4, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.decline_reason_flags (flag_code, reason_id, created_at) VALUES (225, 13, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.decline_reason_flags (flag_code, reason_id, created_at) VALUES (227, 30, NOW());");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM autodecision_core.decline_reason_flags WHERE flag_code = 21 AND reason_id = 29;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.decline_reason_flags WHERE flag_code = 194 AND reason_id = 13;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.decline_reason_flags WHERE flag_code = 220 AND reason_id = 4;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.decline_reason_flags WHERE flag_code = 225 AND reason_id = 13;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.decline_reason_flags WHERE flag_code = 227 AND reason_id = 30;");
        }
    }
}
