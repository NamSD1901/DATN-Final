using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockTimeAndScheduleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "doctor_schedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "recurring_group_id",
                table: "doctor_schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "block_times",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    block_type = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    background_color = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_block_times", x => x.id);
                    table.ForeignKey(
                        name: "FK_block_times_users_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_block_times_doctor_id",
                table: "block_times",
                column: "doctor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "block_times");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "doctor_schedules");

            migrationBuilder.DropColumn(
                name: "recurring_group_id",
                table: "doctor_schedules");
        }
    }
}
