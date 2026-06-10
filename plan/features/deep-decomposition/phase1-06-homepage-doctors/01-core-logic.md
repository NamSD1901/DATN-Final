# 🧠 Core Business Logic & Caching Policy - Vets Team (C# .NET)

Tài liệu này đặc tả logic nghiệp vụ truy vấn thông tin bác sĩ, thiết lập cơ chế bộ đệm IMemoryCache ở Backend và cơ chế dọn dẹp cache chủ động.

---

## 1. Logic Truy vấn & Lưu đệm danh sách Bác sĩ (`DoctorService.cs`)

### 1.1. Luồng xử lý nghiệp vụ chính
*   **Memory Cache Key:** Sử dụng khóa duy nhất `"Doctors_All_Cache"`.
*   **Thời gian lưu đệm:** Absolute Expiration: 1 giờ; Sliding Expiration: 15 phút.
*   **Lọc dữ liệu tại Bộ nhớ:** Lấy toàn bộ danh sách bác sĩ đang hoạt động từ cache, sau đó sử dụng LINQ to Objects để lọc theo chuyên khoa (Specialty) nhằm triệt tiêu tải trọng truy vấn SQL xuống database PostgreSQL.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Common.Interfaces;

namespace MyPetClinic.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string DoctorsCacheKey = "Doctors_All_Cache";

        public DoctorService(IApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<DoctorResponseDto>> GetActiveDoctorsAsync(string? specialtyCode)
        {
            // 1. Kiểm tra xem dữ liệu có trong Cache chưa
            if (!_cache.TryGetValue(DoctorsCacheKey, out List<DoctorResponseDto> cachedDoctors))
            {
                // 2. Cache Miss: Truy vấn Database PostgreSQL (JOIN bảng Doctors và Users)
                cachedDoctors = await _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.User.IsActive)
                    .Select(d => new DoctorResponseDto
                    {
                        Id = d.Id,
                        UserId = d.UserId,
                        FullName = d.User.FullName,
                        Email = d.User.Email,
                        Specialty = d.Specialty.ToString(),
                        SpecialtyCode = GetSpecialtyCode(d.Specialty),
                        ExperienceYears = d.ExperienceYears,
                        Qualifications = d.Qualifications,
                        Biography = d.Biography,
                        AvatarUrl = d.User.AvatarUrl ?? string.Empty,
                        IsOnDuty = d.IsOnDuty
                    })
                    .ToListAsync();

                // 3. Thiết lập thông số Cache
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                    .SetPriority(CacheItemPriority.High);

                // 4. Lưu vào Cache
                _cache.Set(DoctorsCacheKey, cachedDoctors, cacheEntryOptions);
            }

            // 5. Thực hiện Lọc theo Chuyên khoa trên CPU Memory
            var query = cachedDoctors.AsEnumerable();

            if (!string.IsNullOrEmpty(specialtyCode))
            {
                query = query.Where(d => d.SpecialtyCode.Equals(specialtyCode.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query.ToList();
        }

        public void ClearDoctorsCache()
        {
            _cache.Remove(DoctorsCacheKey);
        }

        private string GetSpecialtyCode(Specialty specialty)
        {
            return specialty switch
            {
                Specialty.NoiKhoa => "noi-khoa",
                Specialty.NgoaiKhoa => "ngoai-khoa",
                Specialty.DaLieu => "da-lieu",
                Specialty.TiemChung => "tiem-phong",
                Specialty.ChanDoanHinhAnh => "chan-doan-anh",
                _ => "khac"
            };
        }
    }
}
```

---

## 2. Đồng bộ hóa Thay đổi thông tin Bác sĩ (Cache Eviction)
*   **Vấn đề:** Khi một bác sĩ đổi trạng thái từ trực ca sang nghỉ phép (IsOnDuty = false), nếu không xóa cache, khách hàng vẫn thấy bác sĩ đang trực và tiến hành đặt lịch, dẫn đến lỗi đặt lịch không thể thực hiện.
*   **Giải pháp:** Đăng ký hàm xóa cache chủ động `ClearDoctorsCache()` vào tất cả các API cập nhật thông tin bác sĩ (Update Profile, Cập nhật trạng thái lịch trực của Lễ tân/Admin). Ngay sau khi lưu DB thành công, Cache bị xóa sạch để đảm bảo dữ liệu hiển thị trên trang chủ luôn là mới nhất.
