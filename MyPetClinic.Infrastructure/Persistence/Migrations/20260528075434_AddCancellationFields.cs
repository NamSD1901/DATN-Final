using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    public partial class AddCancellationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancelled_by_role",
                table: "appointments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "cancelled_by_role",
                table: "appointments");
        }
    }
}
