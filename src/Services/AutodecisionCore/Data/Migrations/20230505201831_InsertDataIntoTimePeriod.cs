using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InsertDataIntoTimePeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO autodecision_core.time_periods (description, unit_time, `interval`, is_default, created_at) VALUES ('15 MIN', 15, 1, 1, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.time_periods (description, unit_time, `interval`, is_default, created_at) VALUES ('30 MIN', 30, 1, 0, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.time_periods (description, unit_time, `interval`, is_default, created_at) VALUES ('1 HOUR', 60, 5, 0, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.time_periods (description, unit_time, `interval`, is_default, created_at) VALUES ('2 HOUR', 120, 15, 0, NOW());");
            migrationBuilder.Sql("INSERT INTO autodecision_core.time_periods (description, unit_time, `interval`, is_default, created_at) VALUES ('6 HOUR', 360, 30, 0, NOW());");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM autodecision_core.time_periods WHERE unit_time = 15;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.time_periods WHERE unit_time = 30;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.time_periods WHERE unit_time = 60;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.time_periods WHERE unit_time = 120;");
            migrationBuilder.Sql("DELETE FROM autodecision_core.time_periods WHERE unit_time = 360;");
        }
    }
}
