using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleProfileAndException : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "schedule_exceptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    substitute_doctor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Approved")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_exceptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_schedule_exceptions_users_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_schedule_exceptions_users_substitute_doctor_id",
                        column: x => x.substitute_doctor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "schedule_profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "doctor_schedule_profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<long>(type: "bigint", nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_schedule_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_doctor_schedule_profiles_schedule_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "schedule_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_doctor_schedule_profiles_users_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedule_profile_shifts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    profile_id = table.Column<long>(type: "bigint", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    is_day_off = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_profile_shifts", x => x.id);
                    table.ForeignKey(
                        name: "FK_schedule_profile_shifts_schedule_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "schedule_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_doctor_schedule_profiles_doctor_id",
                table: "doctor_schedule_profiles",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_schedule_profiles_profile_id",
                table: "doctor_schedule_profiles",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_exceptions_doctor_id",
                table: "schedule_exceptions",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_exceptions_substitute_doctor_id",
                table: "schedule_exceptions",
                column: "substitute_doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_profile_shifts_profile_id",
                table: "schedule_profile_shifts",
                column: "profile_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doctor_schedule_profiles");

            migrationBuilder.DropTable(
                name: "schedule_exceptions");

            migrationBuilder.DropTable(
                name: "schedule_profile_shifts");

            migrationBuilder.DropTable(
                name: "schedule_profiles");
        }
    }
}
