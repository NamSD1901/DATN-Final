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
            // 1. Seed Roles
            var rolesToSeed = new[] { "admin", "doctor", "receptionist", "customer" };
            foreach (var roleName in rolesToSeed)
            {
                if (!await context.Roles.AnyAsync(r => r.Name == roleName))
                {
                    context.Roles.Add(new Role { Name = roleName });
                }
            }
            await context.SaveChangesAsync();

            // 2. Seed Default Doctor
            var doctorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "doctor");
            if (doctorRole != null && !await context.Users.AnyAsync(u => u.RoleId == doctorRole.Id))
            {
                var defaultDoctor = new User
                {
                    Id = Guid.NewGuid(),
                    RoleId = doctorRole.Id,
                    FullName = "BS. Trần Văn A",
                    Email = "doctor.a@mypetclinic.com",
                    Phone = "0988888888",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Gender = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(defaultDoctor);
                await context.SaveChangesAsync();
            }

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
                    new Service { CategoryId = categoryKhamBenh.Id, Name = "Khám lâm sàng tổng quát", Price = 100000, DurationMinutes = 30, Description = "Khám tổng quát, nghe tim phổi, kiểm tra nhiệt độ." },
                    new Service { CategoryId = categoryKhamBenh.Id, Name = "Khám chuyên sâu", Price = 200000, DurationMinutes = 45, Description = "Khám chuyên sâu về da liễu, mắt, tai mũi họng." }
                );

                // Dịch vụ Tiêm phòng
                context.Services.AddRange(
                    new Service { CategoryId = categoryTiemPhong.Id, Name = "Tiêm dại", Price = 50000, DurationMinutes = 15, Description = "Tiêm phòng bệnh dại định kỳ hàng năm." },
                    new Service { CategoryId = categoryTiemPhong.Id, Name = "Tiêm vaccine tổng hợp (Chó)", Price = 250000, DurationMinutes = 15, Description = "Vaccine 5 bệnh hoặc 7 bệnh cho chó." },
                    new Service { CategoryId = categoryTiemPhong.Id, Name = "Tiêm vaccine tổng hợp (Mèo)", Price = 300000, DurationMinutes = 15, Description = "Vaccine 4 bệnh cho mèo." }
                );

                // Xét nghiệm
                context.Services.AddRange(
                    new Service { CategoryId = categoryXetNghiem.Id, Name = "Siêu âm ổ bụng", Price = 150000, DurationMinutes = 30, Description = "Siêu âm kiểm tra nội tạng, thai kì." },
                    new Service { CategoryId = categoryXetNghiem.Id, Name = "Xét nghiệm máu tổng quát", Price = 400000, DurationMinutes = 60, Description = "Phân tích sinh hóa và huyết học." }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
