using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutodecisionCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AutoDecisionCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "flags",
                newName: "Flags");

            migrationBuilder.CreateTable(
                name: "application_cores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    loannumber = table.Column<string>(name: "loan_number", type: "varchar(20)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    processingversion = table.Column<int>(name: "processing_version", type: "int", maxLength: 10, nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 2, nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "datetime(6)", nullable: false),
                    isdeleted = table.Column<bool>(name: "is_deleted", type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_cores", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "application_flags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    flagcode = table.Column<string>(name: "flag_code", type: "varchar(4)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    applicationcoreid = table.Column<int>(name: "application_core_id", type: "int", nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requestedat = table.Column<DateTime>(name: "requested_at", type: "datetime(6)", nullable: false),
                    processedat = table.Column<DateTime>(name: "processed_at", type: "datetime(6)", nullable: false),
                    approvedat = table.Column<DateTime>(name: "approved_at", type: "datetime(6)", nullable: false),
                    approvedby = table.Column<int>(name: "approved_by", type: "int", nullable: false),
                    approvalnote = table.Column<string>(name: "approval_note", type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    createdat = table.Column<DateTime>(name: "created_at", type: "datetime(6)", nullable: false),
                    isdeleted = table.Column<bool>(name: "is_deleted", type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_flags", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_flags_application_cores_application_core_id",
                        column: x => x.applicationcoreid,
                        principalTable: "application_cores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "application_processes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    applicationcoreid = table.Column<int>(name: "application_core_id", type: "int", nullable: false),
                    processingversion = table.Column<int>(name: "processing_version", type: "int", maxLength: 10, nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 2, nullable: false),
                    processedat = table.Column<DateTime>(name: "processed_at", type: "datetime(6)", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "datetime(6)", nullable: false),
                    isdeleted = table.Column<bool>(name: "is_deleted", type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_processes", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_processes_application_cores_application_core_id",
                        column: x => x.applicationcoreid,
                        principalTable: "application_cores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "auto_approval_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    applicationcoreid = table.Column<int>(name: "application_core_id", type: "int", nullable: false),
                    rulename = table.Column<string>(name: "rule_name", type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", maxLength: 2, nullable: false),
                    processedat = table.Column<DateTime>(name: "processed_at", type: "datetime(6)", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "datetime(6)", nullable: false),
                    isdeleted = table.Column<bool>(name: "is_deleted", type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auto_approval_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_auto_approval_rules_application_cores_application_core_id",
                        column: x => x.applicationcoreid,
                        principalTable: "application_cores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "application_flags_internal_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    applicationflagid = table.Column<int>(name: "application_flag_id", type: "int", nullable: false),
                    message = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", maxLength: 2, nullable: false),
                    processedat = table.Column<DateTime>(name: "processed_at", type: "datetime(6)", nullable: false),
                    createdat = table.Column<DateTime>(name: "created_at", type: "datetime(6)", nullable: false),
                    isdeleted = table.Column<bool>(name: "is_deleted", type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_flags_internal_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_flags_internal_messages_application_flags_applic",
                        column: x => x.applicationflagid,
                        principalTable: "application_flags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationsCore_LoanNumber",
                table: "application_cores",
                column: "loan_number");

            migrationBuilder.CreateIndex(
                name: "ix_application_flags_application_core_id",
                table: "application_flags",
                column: "application_core_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_flags_internal_messages_application_flag_id",
                table: "application_flags_internal_messages",
                column: "application_flag_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_processes_application_core_id",
                table: "application_processes",
                column: "application_core_id");

            migrationBuilder.CreateIndex(
                name: "ix_auto_approval_rules_application_core_id",
                table: "auto_approval_rules",
                column: "application_core_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_flags_internal_messages");

            migrationBuilder.DropTable(
                name: "application_processes");

            migrationBuilder.DropTable(
                name: "auto_approval_rules");

            migrationBuilder.DropTable(
                name: "application_flags");

            migrationBuilder.DropTable(
                name: "application_cores");

            migrationBuilder.RenameTable(
                name: "Flags",
                newName: "flags");
        }
    }
}
