using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cột deleted_at cho reviews đã tồn tại trong database, bỏ qua để tránh lỗi.

            migrationBuilder.AddColumn<bool>(
                name: "is_system_generated",
                table: "appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "reference_record_id",
                table: "appointments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reminder_status",
                table: "appointments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "appointments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Normal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "deleted_at",
            //     table: "reviews");

            migrationBuilder.DropColumn(
                name: "is_system_generated",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "reference_record_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "reminder_status",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "type",
                table: "appointments");
        }
    }
}
