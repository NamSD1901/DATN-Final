using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVaccinationSoapSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropIndex(
            //     name: "IX_vaccination_records_appointment_id",
            //     table: "vaccination_records");

            migrationBuilder.AlterColumn<long>(
                name: "vaccine_id",
                table: "vaccination_records",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "reaction_note",
                table: "vaccination_records",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "injection_date",
                table: "vaccination_records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "allergy_details",
                table: "vaccination_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "clinical_assessment",
                table: "vaccination_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "vaccination_records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<int>(
                name: "dehydration_percent",
                table: "vaccination_records",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "doctor_remarks",
                table: "vaccination_records",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "dose",
                table: "vaccination_records",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "eating_status",
                table: "vaccination_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "eye_nose_ear_status",
                table: "vaccination_records",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "follow_up_instructions",
                table: "vaccination_records",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "has_cough_or_sneeze",
                table: "vaccination_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_previous_reaction",
                table: "vaccination_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_vomiting_or_diarrhea",
                table: "vaccination_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "heart_rate",
                table: "vaccination_records",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "injection_site",
                table: "vaccination_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_allergic",
                table: "vaccination_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_under_treatment",
                table: "vaccination_records",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "lymph_node_status",
                table: "vaccination_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mental_status",
                table: "vaccination_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mucosa_status",
                table: "vaccination_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "owner_notes",
                table: "vaccination_records",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_reaction_details",
                table: "vaccination_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_vaccine_history",
                table: "vaccination_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reason_for_visit",
                table: "vaccination_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "respiratory_rate",
                table: "vaccination_records",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "route",
                table: "vaccination_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "temperature",
                table: "vaccination_records",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "treatment_details",
                table: "vaccination_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "vaccine_batch_id",
                table: "vaccination_records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "weight",
                table: "vaccination_records",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "vaccine_batches",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    vaccine_id = table.Column<long>(type: "bigint", nullable: false),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    import_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    import_price = table.Column<decimal>(type: "numeric", nullable: false),
                    selling_price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccine_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_vaccine_batches_vaccines_vaccine_id",
                        column: x => x.vaccine_id,
                        principalTable: "vaccines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_records_appointment_id",
                table: "vaccination_records",
                column: "appointment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_records_vaccine_batch_id",
                table: "vaccination_records",
                column: "vaccine_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_vaccine_batches_vaccine_id",
                table: "vaccine_batches",
                column: "vaccine_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vaccination_records_vaccine_batches_vaccine_batch_id",
                table: "vaccination_records",
                column: "vaccine_batch_id",
                principalTable: "vaccine_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vaccination_records_vaccine_batches_vaccine_batch_id",
                table: "vaccination_records");

            migrationBuilder.DropTable(
                name: "vaccine_batches");

            migrationBuilder.DropIndex(
                name: "IX_vaccination_records_appointment_id",
                table: "vaccination_records");

            migrationBuilder.DropIndex(
                name: "IX_vaccination_records_vaccine_batch_id",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "allergy_details",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "clinical_assessment",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "dehydration_percent",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "doctor_remarks",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "dose",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "eating_status",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "eye_nose_ear_status",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "follow_up_instructions",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "has_cough_or_sneeze",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "has_previous_reaction",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "has_vomiting_or_diarrhea",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "heart_rate",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "injection_site",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "is_allergic",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "is_under_treatment",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "lymph_node_status",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "mental_status",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "mucosa_status",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "owner_notes",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "previous_reaction_details",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "previous_vaccine_history",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "reason_for_visit",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "respiratory_rate",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "route",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "temperature",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "treatment_details",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "vaccine_batch_id",
                table: "vaccination_records");

            migrationBuilder.DropColumn(
                name: "weight",
                table: "vaccination_records");

            migrationBuilder.AlterColumn<long>(
                name: "vaccine_id",
                table: "vaccination_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "reaction_note",
                table: "vaccination_records",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "injection_date",
                table: "vaccination_records",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_records_appointment_id",
                table: "vaccination_records",
                column: "appointment_id");
        }
    }
}
