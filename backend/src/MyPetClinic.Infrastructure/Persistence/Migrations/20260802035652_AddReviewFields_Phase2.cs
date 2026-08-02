using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewFields_Phase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "clinic_reply",
                table: "reviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "helpful_count",
                table: "reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "image_urls",
                table: "reviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "like_count",
                table: "reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "replied_at",
                table: "reviews",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "clinic_reply",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "helpful_count",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "image_urls",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "is_verified",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "like_count",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "replied_at",
                table: "reviews");
        }
    }
}
