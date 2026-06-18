using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPetClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalRecordSOAP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "heart_rate",
                table: "medical_records");

            //migrationBuilder.RenameColumn(
            //    name: "DeletedAt",
            //    table: "users",
            //    newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "symptoms",
                table: "medical_records",
                newName: "medical_history");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "medical_records",
                newName: "doctor_notes");

            //migrationBuilder.RenameColumn(
            //    name: "QueueNumber",
            //    table: "appointments",
            //    newName: "queue_number");

            //migrationBuilder.RenameColumn(
            //    name: "QrToken",
            //    table: "appointments",
            //    newName: "qr_token");

            //migrationBuilder.RenameColumn(
            //    name: "IsWalkIn",
            //    table: "appointments",
            //    newName: "is_walk_in");

            //migrationBuilder.RenameColumn(
            //    name: "IsEmergency",
            //    table: "appointments",
            //    newName: "is_emergency");

            //migrationBuilder.RenameColumn(
            //    name: "CheckOutTime",
            //    table: "appointments",
            //    newName: "check_out_time");

            //migrationBuilder.RenameColumn(
            //    name: "CheckInTime",
            //    table: "appointments",
            //    newName: "check_in_time");

            //migrationBuilder.AddColumn<int>(
            //    name: "interval_days",
            //    table: "vaccines",
            //    type: "integer",
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "min_age_weeks",
            //    table: "vaccines",
            //    type: "integer",
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "stock_quantity",
            //    table: "vaccines",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "target_species",
            //    table: "vaccines",
            //    type: "character varying(50)",
            //    maxLength: 50,
            //    nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "weight",
                table: "medical_records",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "treatment_plan",
                table: "medical_records",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "temperature",
                table: "medical_records",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "diagnosis",
                table: "medical_records",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "clinical_signs",
                table: "medical_records",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "pet_id",
                table: "medical_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("UPDATE medical_records SET pet_id = (SELECT pet_id FROM appointments WHERE appointments.id = medical_records.appointment_id);");

            migrationBuilder.AddColumn<string>(
                name: "record_type",
                table: "medical_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Consultation");

            //migrationBuilder.AlterColumn<int>(
            //    name: "queue_number",
            //    table: "appointments",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0,
            //    oldClrType: typeof(int),
            //    oldType: "integer");

            //migrationBuilder.AlterColumn<bool>(
            //    name: "is_walk_in",
            //    table: "appointments",
            //    type: "boolean",
            //    nullable: false,
            //    defaultValue: false,
            //    oldClrType: typeof(bool),
            //    oldType: "boolean");

            //migrationBuilder.AlterColumn<bool>(
            //    name: "is_emergency",
            //    table: "appointments",
            //    type: "boolean",
            //    nullable: false,
            //    defaultValue: false,
            //    oldClrType: typeof(bool),
            //    oldType: "boolean");

            //migrationBuilder.AddColumn<string>(
            //    name: "cancel_reason",
            //    table: "appointments",
            //    type: "text",
            //    nullable: true);

            //migrationBuilder.AddColumn<long>(
            //    name: "vaccine_id",
            //    table: "appointments",
            //    type: "bigint",
            //    nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_pet_id",
                table: "medical_records",
                column: "pet_id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_appointments_vaccine_id",
            //    table: "appointments",
            //    column: "vaccine_id");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_appointments_vaccines_vaccine_id",
            //    table: "appointments",
            //    column: "vaccine_id",
            //    principalTable: "vaccines",
            //    principalColumn: "id",
            //    onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_medical_records_pets_pet_id",
                table: "medical_records",
                column: "pet_id",
                principalTable: "pets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_vaccines_vaccine_id",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_medical_records_pets_pet_id",
                table: "medical_records");

            migrationBuilder.DropIndex(
                name: "IX_medical_records_pet_id",
                table: "medical_records");

            migrationBuilder.DropIndex(
                name: "IX_appointments_vaccine_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "interval_days",
                table: "vaccines");

            migrationBuilder.DropColumn(
                name: "min_age_weeks",
                table: "vaccines");

            migrationBuilder.DropColumn(
                name: "stock_quantity",
                table: "vaccines");

            migrationBuilder.DropColumn(
                name: "target_species",
                table: "vaccines");

            migrationBuilder.DropColumn(
                name: "clinical_signs",
                table: "medical_records");

            migrationBuilder.DropColumn(
                name: "pet_id",
                table: "medical_records");

            migrationBuilder.DropColumn(
                name: "record_type",
                table: "medical_records");

            migrationBuilder.DropColumn(
                name: "cancel_reason",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "vaccine_id",
                table: "appointments");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "users",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "medical_history",
                table: "medical_records",
                newName: "symptoms");

            migrationBuilder.RenameColumn(
                name: "doctor_notes",
                table: "medical_records",
                newName: "note");

            migrationBuilder.RenameColumn(
                name: "queue_number",
                table: "appointments",
                newName: "QueueNumber");

            migrationBuilder.RenameColumn(
                name: "qr_token",
                table: "appointments",
                newName: "QrToken");

            migrationBuilder.RenameColumn(
                name: "is_walk_in",
                table: "appointments",
                newName: "IsWalkIn");

            migrationBuilder.RenameColumn(
                name: "is_emergency",
                table: "appointments",
                newName: "IsEmergency");

            migrationBuilder.RenameColumn(
                name: "check_out_time",
                table: "appointments",
                newName: "CheckOutTime");

            migrationBuilder.RenameColumn(
                name: "check_in_time",
                table: "appointments",
                newName: "CheckInTime");

            migrationBuilder.AlterColumn<decimal>(
                name: "weight",
                table: "medical_records",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "treatment_plan",
                table: "medical_records",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "temperature",
                table: "medical_records",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "diagnosis",
                table: "medical_records",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "heart_rate",
                table: "medical_records",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "QueueNumber",
                table: "appointments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsWalkIn",
                table: "appointments",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEmergency",
                table: "appointments",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
