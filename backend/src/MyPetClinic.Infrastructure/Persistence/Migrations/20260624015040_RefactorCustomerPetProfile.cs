using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCustomerPetProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_appointments_users_customer_id",
            //     table: "appointments");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_pets_users_owner_id",
            //     table: "pets");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_reviews_users_customer_id",
            //     table: "reviews");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    customer_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<short>(type: "smallint", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    has_account = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "Active"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                    table.ForeignKey(
                        name: "FK_customers_users_AccountId",
                        column: x => x.AccountId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.Sql(@"
                INSERT INTO customers (id, customer_code, full_name, email, phone, avatar, gender, date_of_birth, address, has_account, status, created_at, updated_at, deleted_at, ""AccountId"")
                SELECT id, NULL, full_name, email, phone, avatar, gender, date_of_birth, address, true, CASE WHEN is_active THEN 'Active' ELSE 'Inactive' END, created_at, NULL, deleted_at, id
                FROM users
                WHERE id IN (
                    SELECT DISTINCT customer_id FROM appointments 
                    UNION SELECT DISTINCT owner_id FROM pets
                    UNION SELECT DISTINCT customer_id FROM reviews
                    UNION SELECT id FROM users WHERE role_id = (SELECT id FROM roles WHERE name = 'Customer' LIMIT 1)
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_users_CustomerId",
                table: "users",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_AccountId",
                table: "customers",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_email",
                table: "customers",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pets_customers_owner_id",
                table: "pets",
                column: "owner_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_customers_customer_id",
                table: "reviews",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_customers_CustomerId",
                table: "users",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_pets_customers_owner_id",
                table: "pets");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_customers_customer_id",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_users_customers_CustomerId",
                table: "users");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropIndex(
                name: "IX_users_CustomerId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "users");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_users_customer_id",
                table: "appointments",
                column: "customer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pets_users_owner_id",
                table: "pets",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_users_customer_id",
                table: "reviews",
                column: "customer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
