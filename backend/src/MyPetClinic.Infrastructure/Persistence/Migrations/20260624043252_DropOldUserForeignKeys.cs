using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropOldUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE pets DROP CONSTRAINT IF EXISTS pets_owner_id_fkey;");
            migrationBuilder.Sql("ALTER TABLE appointments DROP CONSTRAINT IF EXISTS appointments_customer_id_fkey;");
            migrationBuilder.Sql("ALTER TABLE reviews DROP CONSTRAINT IF EXISTS reviews_customer_id_fkey;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
