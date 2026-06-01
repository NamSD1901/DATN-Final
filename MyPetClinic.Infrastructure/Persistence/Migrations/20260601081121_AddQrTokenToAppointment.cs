using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQrTokenToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrToken",
                table: "appointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "appointments");
        }
    }
}
