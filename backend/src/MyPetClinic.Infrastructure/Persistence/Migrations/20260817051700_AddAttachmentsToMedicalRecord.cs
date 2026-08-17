using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentsToMedicalRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Attachments",
                table: "medical_records",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attachments",
                table: "medical_records");
        }
    }
}
