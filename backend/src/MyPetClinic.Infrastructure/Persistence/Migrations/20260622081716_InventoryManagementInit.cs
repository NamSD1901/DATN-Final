using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InventoryManagementInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "medicines");

            migrationBuilder.RenameColumn(
                name: "stock_quantity",
                table: "medicines",
                newName: "min_stock_level");

            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "medicines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "sell_price",
                table: "medicines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "import_price",
                table: "medicines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "category_id",
                table: "medicines",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "medicines",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "medicine_code",
                table: "medicines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "medicine_batches",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    medicine_id = table.Column<long>(type: "bigint", nullable: false),
                    manufacture_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    initial_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    current_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicine_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_medicine_batches_medicines_medicine_id",
                        column: x => x.medicine_id,
                        principalTable: "medicines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medicine_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicine_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    type = table.Column<int>(type: "integer", nullable: false),
                    medicine_id = table.Column<long>(type: "bigint", nullable: false),
                    batch_id = table.Column<long>(type: "bigint", nullable: true),
                    quantity_change = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_medicine_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "medicine_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_medicines_medicine_id",
                        column: x => x.medicine_id,
                        principalTable: "medicines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_medicines_category_id",
                table: "medicines",
                column: "category_id");

            migrationBuilder.Sql("UPDATE medicines SET medicine_code = 'MED' || id::text WHERE medicine_code = '' OR medicine_code IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_medicines_medicine_code",
                table: "medicines",
                column: "medicine_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_batch_id",
                table: "inventory_transactions",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_medicine_id",
                table: "inventory_transactions",
                column: "medicine_id");

            migrationBuilder.CreateIndex(
                name: "IX_medicine_batches_medicine_id",
                table: "medicine_batches",
                column: "medicine_id");

            migrationBuilder.Sql("INSERT INTO medicine_categories (name, description) VALUES ('Chưa phân loại', 'Danh mục mặc định');");
            migrationBuilder.Sql("UPDATE medicines SET category_id = (SELECT id FROM medicine_categories LIMIT 1) WHERE category_id = 0;");

            migrationBuilder.AddForeignKey(
                name: "FK_medicines_medicine_categories_category_id",
                table: "medicines",
                column: "category_id",
                principalTable: "medicine_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_medicines_medicine_categories_category_id",
                table: "medicines");

            migrationBuilder.DropTable(
                name: "inventory_transactions");

            migrationBuilder.DropTable(
                name: "medicine_categories");

            migrationBuilder.DropTable(
                name: "medicine_batches");

            migrationBuilder.DropIndex(
                name: "IX_medicines_category_id",
                table: "medicines");

            migrationBuilder.DropIndex(
                name: "IX_medicines_medicine_code",
                table: "medicines");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "medicines");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "medicines");

            migrationBuilder.DropColumn(
                name: "medicine_code",
                table: "medicines");

            migrationBuilder.RenameColumn(
                name: "min_stock_level",
                table: "medicines",
                newName: "stock_quantity");

            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "medicines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "sell_price",
                table: "medicines",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "import_price",
                table: "medicines",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "expiry_date",
                table: "medicines",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
