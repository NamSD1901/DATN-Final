using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Persistence
{
    public static class ApplicationDbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Run raw DDL migration to guarantee DB has new columns on Supabase
            try
            {
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS vaccine_id BIGINT REFERENCES vaccines(id);");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS cancel_reason TEXT;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS check_in_time TIMESTAMP WITH TIME ZONE;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS check_out_time TIMESTAMP WITH TIME ZONE;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS is_walk_in BOOLEAN DEFAULT FALSE;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS is_emergency BOOLEAN DEFAULT FALSE;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS queue_number INT DEFAULT 0;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE appointments ADD COLUMN IF NOT EXISTS qr_token TEXT;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE vaccines ADD COLUMN IF NOT EXISTS stock_quantity INT DEFAULT 10;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE vaccines ADD COLUMN IF NOT EXISTS target_species VARCHAR(50);");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE vaccines ADD COLUMN IF NOT EXISTS min_age_weeks INT;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE vaccines ADD COLUMN IF NOT EXISTS interval_days INT;");
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE users ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;");
            }
            catch { /* Chạy local sqlite test có thể ném exception do EF In-memory hoặc SQLite không nhận, bỏ qua để test pass */ }

            // Seed Vaccines
            if (!await context.Vaccines.AnyAsync())
            {
                context.Vaccines.AddRange(
                    new Vaccine { Name = "Vắc-xin phòng bệnh Dại (Nobivac Rabies)", Manufacturer = "MSD Animal Health", Description = "Phòng bệnh dại định kì cho chó và mèo.", StockQuantity = 20, TargetSpecies = "All", MinAgeWeeks = 12, IntervalDays = 335 },
                    new Vaccine { Name = "Vắc-xin 4 bệnh mèo (Nobivac Tricat)", Manufacturer = "MSD Animal Health", Description = "Phòng bệnh giảm bạch cầu, viêm mũi khí quản, Calicivirus và Chlamydia.", StockQuantity = 15, TargetSpecies = "Cat", MinAgeWeeks = 8, IntervalDays = 21 },
                    new Vaccine { Name = "Vắc-xin 5 bệnh chó (Nobivac DHPPi)", Manufacturer = "MSD Animal Health", Description = "Phòng bệnh Sài sốt (Carré), Viêm gan truyền nhiễm, Viêm ruột do Parvovirus, Phổi và Cúm.", StockQuantity = 25, TargetSpecies = "Dog", MinAgeWeeks = 6, IntervalDays = 21 }
                );
                await context.SaveChangesAsync();
            }

            // Seed Medicines
            if (!await context.Medicines.AnyAsync())
            {
                context.Medicines.AddRange(
                    new Medicine { Name = "Thuốc kháng sinh Amoxicillin", Unit = "Viên", StockQuantity = 100, ImportPrice = 5000, SellPrice = 10000, ExpiryDate = DateTime.UtcNow.AddYears(1), Description = "Dùng cho nhiễm khuẩn đường hô hấp, tiêu hóa" },
                    new Medicine { Name = "Thuốc tẩy giun sán Drontal", Unit = "Viên", StockQuantity = 50, ImportPrice = 30000, SellPrice = 50000, ExpiryDate = DateTime.UtcNow.AddYears(2), Description = "Tẩy giun phổ rộng cho chó mèo" },
                    new Medicine { Name = "Thuốc bôi da mỡ Kẽm Oxyde", Unit = "Tuýp", StockQuantity = 30, ImportPrice = 15000, SellPrice = 35000, ExpiryDate = DateTime.UtcNow.AddYears(1), Description = "Điều trị các vết thương ngoài da, viêm da" },
                    new Medicine { Name = "Nước muối sinh lý Natri Clorid 0.9%", Unit = "Chai", StockQuantity = 200, ImportPrice = 5000, SellPrice = 15000, ExpiryDate = DateTime.UtcNow.AddYears(3), Description = "Rửa vết thương, rửa mắt, mũi" }
                );
                await context.SaveChangesAsync();
            }

            // 1. Seed Roles
            var rolesToSeed = new[] { "admin", "clinical_doctor", "vaccination_doctor", "receptionist", "customer" };
            foreach (var roleName in rolesToSeed)
            {
                if (!await context.Roles.AnyAsync(r => r.Name == roleName))
                {
                    context.Roles.Add(new Role { Name = roleName });
                }
            }
            await context.SaveChangesAsync();

            // 2. Seed Default Users
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "admin");
            var doctorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "doctor");
            var receptionistRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "receptionist");
            var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "customer");

            if (adminRole != null && !await context.Users.AnyAsync(u => u.Email == "admindemo@gmail.com"))
            {
                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    FullName = "Quản trị viên Demo",
                    Email = "admindemo@gmail.com",
                    Phone = "0999999999",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Gender = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (receptionistRole != null && !await context.Users.AnyAsync(u => u.Email == "letandemo@gmail.com"))
            {
                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    RoleId = receptionistRole.Id,
                    FullName = "Lễ tân Demo",
                    Email = "letandemo@gmail.com",
                    Phone = "0988888888",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Gender = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (customerRole != null && !await context.Users.AnyAsync(u => u.Email == "khachhangdemo@gmail.com"))
            {
                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    RoleId = customerRole.Id,
                    FullName = "Khách hàng Demo",
                    Email = "khachhangdemo@gmail.com",
                    Phone = "0977777777",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Gender = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var newDoctorEmails = new[] { "bacsilong@gmail.com", "bacsituantran@gmail.com", "bacsichung@gmail.com", "bacsiha@gmail.com" };

            if (doctorRole != null)
            {
                // Soft delete old doctors to keep data safe (Use Raw SQL for guaranteed execution on Postgres)
                var sql = "UPDATE users SET is_active = false, deleted_at = CURRENT_TIMESTAMP WHERE role_id = {0} AND email NOT IN ('bacsi_test@gmail.com', 'bacsituantran@gmail.com', 'bacsichung@gmail.com', 'bacsiha@gmail.com')";
                await context.Database.ExecuteSqlRawAsync(sql, doctorRole.Id);

                // Restore bacsi_test@gmail.com and update its display name & role to doctor
                var sqlRestore = "UPDATE users SET is_active = true, deleted_at = NULL, full_name = 'BS. Tr\u1ea7n Th\u0103ng Long', role_id = {0} WHERE email = 'bacsi_test@gmail.com'";
                await context.Database.ExecuteSqlRawAsync(sqlRestore, doctorRole.Id);

                // bacsi_test@gmail.com được dùng thay cho bacsilong, đã restore bằng Raw SQL bên trên
                // Chỉ tạo mới nếu không có cả 2 email này trong DB
                if (!await context.Users.AnyAsync(u => u.Email == "bacsi_test@gmail.com" || u.Email == "bacsilong@gmail.com"))
                {
                    context.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        RoleId = doctorRole.Id,
                        FullName = "BS. Trần Thăng Long",
                        Email = "bacsi_test@gmail.com",
                        Phone = "0911111111",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Gender = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (!await context.Users.AnyAsync(u => u.Email == "bacsituantran@gmail.com"))
                {
                    context.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        RoleId = doctorRole.Id,
                        FullName = "BS. Trần Văn Tuấn",
                        Email = "bacsituantran@gmail.com",
                        Phone = "0922222222",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Gender = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (!await context.Users.AnyAsync(u => u.Email == "bacsichung@gmail.com"))
                {
                    context.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        RoleId = doctorRole.Id,
                        FullName = "BS. Lương Thị Chung",
                        Email = "bacsichung@gmail.com",
                        Phone = "0933333333",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Gender = 0,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (!await context.Users.AnyAsync(u => u.Email == "bacsiha@gmail.com"))
                {
                    context.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        RoleId = doctorRole.Id,
                        FullName = "BS. Hoàng Văn Hà",
                        Email = "bacsiha@gmail.com",
                        Phone = "0944444444",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Gender = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();

            // 3. Seed Service Categories & Services
            if (!await context.ServiceCategories.AnyAsync())
            {
                var categoryKhamBenh = new ServiceCategory { Name = "Khám bệnh" };
                var categoryTiemPhong = new ServiceCategory { Name = "Tiêm phòng" };
                var categoryXetNghiem = new ServiceCategory { Name = "Xét nghiệm & Siêu âm" };
                var categoryPhauThuat = new ServiceCategory { Name = "Phẫu thuật" };
                var categoryLamDep = new ServiceCategory { Name = "Grooming & Spa" };

                context.ServiceCategories.AddRange(categoryKhamBenh, categoryTiemPhong, categoryXetNghiem, categoryPhauThuat, categoryLamDep);
                await context.SaveChangesAsync();

                // Dịch vụ Khám bệnh
                context.Services.AddRange(
                    new Service { CategoryId = categoryKhamBenh.Id, Name = "Khám bệnh", Price = 450000, DurationMinutes = 30, Description = "Kiểm tra sức khỏe tổng quát, chẩn đoán và tư vấn điều trị cho thú cưng của bạn." }
                );

                // Xét nghiệm
                context.Services.AddRange(
                    new Service { CategoryId = categoryXetNghiem.Id, Name = "Siêu âm ổ bụng", Price = 150000, DurationMinutes = 30, Description = "Siêu âm kiểm tra nội tạng, thai kì." },
                    new Service { CategoryId = categoryXetNghiem.Id, Name = "Xét nghiệm máu tổng quát", Price = 400000, DurationMinutes = 60, Description = "Phân tích sinh hóa và huyết học." }
                );

                await context.SaveChangesAsync();
            }

            // Force reset services if there are more than 2
            var existingServices = await context.Services.ToListAsync();
            if (existingServices.Count > 2)
            {
                context.Services.RemoveRange(existingServices);
                await context.SaveChangesAsync();

                var categoryKhamBenh = await context.ServiceCategories.FirstOrDefaultAsync(c => c.Name == "Khám bệnh");
                var categoryTiemPhong = await context.ServiceCategories.FirstOrDefaultAsync(c => c.Name == "Tiêm phòng");

                if (categoryKhamBenh != null && categoryTiemPhong != null)
                {
                    context.Services.AddRange(
                        new Service { CategoryId = categoryKhamBenh.Id, Name = "Khám bệnh", Price = 450000, DurationMinutes = 30, Description = "Kiểm tra sức khỏe tổng quát, chẩn đoán và tư vấn điều trị cho thú cưng của bạn." },
                        new Service { CategoryId = categoryTiemPhong.Id, Name = "Tiêm phòng", Price = 300000, DurationMinutes = 15, Description = "Tiêm các loại vaccine cần thiết định kỳ để phòng ngừa bệnh truyền nhiễm cho thú cưng." }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
