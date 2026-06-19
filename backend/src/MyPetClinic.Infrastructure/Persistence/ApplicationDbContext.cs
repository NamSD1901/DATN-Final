using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Infrastructure.Persistence
{
    public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
    {
        public DateTimeUtcConverter() : base(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        { }
    }

    public class NullableDateTimeUtcConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableDateTimeUtcConverter() : base(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        { }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<VaccinationRecord> VaccinationRecords { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateTime>().HaveConversion<DateTimeUtcConverter>();
            builder.Properties<DateTime?>().HaveConversion<NullableDateTimeUtcConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
            });

            // users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Avatar).HasColumnName("avatar");
                entity.Property(e => e.Gender).HasColumnName("gender");
                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

                entity.HasOne(d => d.Role).WithMany(p => p.Users).HasForeignKey(d => d.RoleId).OnDelete(DeleteBehavior.Restrict);
            });

            // pets
            modelBuilder.Entity<Pet>(entity =>
            {
                entity.ToTable("pets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.OwnerId).HasColumnName("owner_id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.Species).HasColumnName("species").HasMaxLength(100);
                entity.Property(e => e.Breed).HasColumnName("breed").HasMaxLength(100);
                entity.Property(e => e.Gender).HasColumnName("gender");
                entity.Property(e => e.BirthDate).HasColumnName("birth_date");
                entity.Property(e => e.Weight).HasColumnName("weight");
                entity.Property(e => e.Color).HasColumnName("color").HasMaxLength(100);
                entity.Property(e => e.BloodType).HasColumnName("blood_type").HasMaxLength(20);
                entity.Property(e => e.Sterilized).HasColumnName("sterilized");
                entity.Property(e => e.MicrochipCode).HasColumnName("microchip_code").HasMaxLength(100);
                entity.Property(e => e.AllergyNote).HasColumnName("allergy_note");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Owner).WithMany().HasForeignKey(d => d.OwnerId).OnDelete(DeleteBehavior.Cascade);
            });

            // service_categories
            modelBuilder.Entity<ServiceCategory>(entity =>
            {
                entity.ToTable("service_categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            });

            // services
            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("services");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(d => d.Category).WithMany(p => p.Services).HasForeignKey(d => d.CategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            // doctor_schedules
            modelBuilder.Entity<DoctorSchedule>(entity =>
            {
                entity.ToTable("doctor_schedules");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
                entity.Property(e => e.WorkDate).HasColumnName("work_date");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.MaxAppointments).HasColumnName("max_appointments").HasDefaultValue(10);
                entity.Property(e => e.IsAvailable).HasColumnName("is_available").HasDefaultValue(true);

                entity.HasOne(d => d.Doctor).WithMany().HasForeignKey(d => d.DoctorId).OnDelete(DeleteBehavior.Cascade);
            });

            // appointments
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("appointments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.PetId).HasColumnName("pet_id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
                entity.Property(e => e.ServiceId).HasColumnName("service_id");
                entity.Property(e => e.VaccineId).HasColumnName("vaccine_id");
                entity.Property(e => e.AppointmentDate).HasColumnName("appointment_date");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("pending").HasMaxLength(50);
                entity.Property(e => e.Symptom).HasColumnName("symptom");
                entity.Property(e => e.Note).HasColumnName("note");
                entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
                entity.Property(e => e.CheckInTime).HasColumnName("check_in_time");
                entity.Property(e => e.CheckOutTime).HasColumnName("check_out_time");
                entity.Property(e => e.IsWalkIn).HasColumnName("is_walk_in").HasDefaultValue(false);
                entity.Property(e => e.IsEmergency).HasColumnName("is_emergency").HasDefaultValue(false);
                entity.Property(e => e.QueueNumber).HasColumnName("queue_number").HasDefaultValue(0);
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(e => e.QrToken).HasColumnName("qr_token");

                entity.HasOne(d => d.Pet).WithMany(p => p.Appointments).HasForeignKey(d => d.PetId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Doctor).WithMany().HasForeignKey(d => d.DoctorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Service).WithMany(p => p.Appointments).HasForeignKey(d => d.ServiceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Vaccine).WithMany().HasForeignKey(d => d.VaccineId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Creator).WithMany().HasForeignKey(d => d.CreatedBy).OnDelete(DeleteBehavior.SetNull);
            });

            // medical_records
            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.ToTable("medical_records");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
                entity.HasIndex(e => e.AppointmentId).IsUnique();
                entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
                entity.Property(e => e.PetId).HasColumnName("pet_id");
                entity.Property(e => e.RecordType).HasColumnName("record_type").HasMaxLength(50).HasDefaultValue("Consultation");
                
                entity.Property(e => e.MedicalHistory).HasColumnName("medical_history");
                entity.Property(e => e.Weight).HasColumnName("weight");
                entity.Property(e => e.Temperature).HasColumnName("temperature");
                entity.Property(e => e.ClinicalSigns).HasColumnName("clinical_signs");
                
                entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
                entity.Property(e => e.TreatmentPlan).HasColumnName("treatment_plan");
                entity.Property(e => e.DoctorNotes).HasColumnName("doctor_notes");
                entity.Property(e => e.FollowUpDate).HasColumnName("follow_up_date");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Appointment).WithOne(p => p.MedicalRecord).HasForeignKey<MedicalRecord>(d => d.AppointmentId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Doctor).WithMany().HasForeignKey(d => d.DoctorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Pet).WithMany().HasForeignKey(d => d.PetId).OnDelete(DeleteBehavior.Restrict);
            });

            // medicines
            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.ToTable("medicines");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(50);
                entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
                entity.Property(e => e.ImportPrice).HasColumnName("import_price");
                entity.Property(e => e.SellPrice).HasColumnName("sell_price");
                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
                entity.Property(e => e.Description).HasColumnName("description");
            });

            // prescriptions
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.ToTable("prescriptions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MedicalRecordId).HasColumnName("medical_record_id");
                entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
                entity.Property(e => e.Note).HasColumnName("note");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.MedicalRecord).WithMany(p => p.Prescriptions).HasForeignKey(d => d.MedicalRecordId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Doctor).WithMany().HasForeignKey(d => d.DoctorId).OnDelete(DeleteBehavior.Restrict);
            });

            // prescription_items
            modelBuilder.Entity<PrescriptionItem>(entity =>
            {
                entity.ToTable("prescription_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
                entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
                entity.Property(e => e.Dosage).HasColumnName("dosage").HasMaxLength(100);
                entity.Property(e => e.Frequency).HasColumnName("frequency").HasMaxLength(100);
                entity.Property(e => e.DurationDays).HasColumnName("duration_days");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.Instruction).HasColumnName("instruction");

                entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionItems).HasForeignKey(d => d.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Medicine).WithMany().HasForeignKey(d => d.MedicineId).OnDelete(DeleteBehavior.Restrict);
            });

            // vaccines
            modelBuilder.Entity<Vaccine>(entity =>
            {
                entity.ToTable("vaccines");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Manufacturer).HasColumnName("manufacturer").HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
                entity.Property(e => e.TargetSpecies).HasColumnName("target_species").HasMaxLength(50);
                entity.Property(e => e.MinAgeWeeks).HasColumnName("min_age_weeks");
                entity.Property(e => e.IntervalDays).HasColumnName("interval_days");
            });

            // vaccination_records
            modelBuilder.Entity<VaccinationRecord>(entity =>
            {
                entity.ToTable("vaccination_records");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.PetId).HasColumnName("pet_id");
                entity.Property(e => e.VaccineId).HasColumnName("vaccine_id");
                entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
                entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
                entity.Property(e => e.InjectionDate).HasColumnName("injection_date");
                entity.Property(e => e.NextDueDate).HasColumnName("next_due_date");
                entity.Property(e => e.ReactionNote).HasColumnName("reaction_note");

                entity.HasOne(d => d.Pet).WithMany(p => p.VaccinationRecords).HasForeignKey(d => d.PetId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Vaccine).WithMany(p => p.VaccinationRecords).HasForeignKey(d => d.VaccineId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Appointment).WithMany().HasForeignKey(d => d.AppointmentId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(d => d.Doctor).WithMany().HasForeignKey(d => d.DoctorId).OnDelete(DeleteBehavior.Restrict);
            });

            // invoices
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoices");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
                entity.HasIndex(e => e.AppointmentId).IsUnique();
                entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasDefaultValue(0);
                entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasDefaultValue(0);
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasDefaultValue(0);
                entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasDefaultValue("unpaid").HasMaxLength(50);
                entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
                entity.Property(e => e.PaidAt).HasColumnName("paid_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Appointment).WithOne(p => p.Invoice).HasForeignKey<Invoice>(d => d.AppointmentId).OnDelete(DeleteBehavior.Restrict);
            });

            // invoice_items
            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.ToTable("invoice_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
                entity.Property(e => e.ItemType).HasColumnName("item_type").HasMaxLength(50);
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(255);
                entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(1);
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasDefaultValue(0);
                entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasDefaultValue(0);

                entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems).HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            });

            // reviews
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
                entity.HasIndex(e => e.AppointmentId).IsUnique();
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Comment).HasColumnName("comment");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Appointment).WithOne(p => p.Review).HasForeignKey<Review>(d => d.AppointmentId).OnDelete(DeleteBehavior.Cascade);
            });

            // posts
            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(255);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Thumbnail).HasColumnName("thumbnail");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.AuthorId).HasColumnName("author_id");
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("draft").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Author).WithMany().HasForeignKey(d => d.AuthorId).OnDelete(DeleteBehavior.SetNull);
            });

            // notifications
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).HasColumnName("content").IsRequired();
                entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
                entity.Property(e => e.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.UserId, e.IsRead }).HasFilter("\"is_read\" = false");
                entity.HasIndex(e => e.CreatedAt).IsDescending();
            });
        }
    }
}
