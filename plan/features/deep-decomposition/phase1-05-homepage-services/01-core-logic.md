# 🧠 Core Business Logic & Cache Manager (C# .NET)

Tài liệu này đặc tả logic nghiệp vụ truy vấn danh sách dịch vụ y tế, áp dụng bộ đệm (Caching) tối ưu hiệu năng và cơ chế lọc kết quả tại tầng ứng dụng.

---

## 1. Logic Truy vấn & Lưu đệm Dịch vụ (`ServiceService.cs`)

### 1.1. Luồng xử lý nghiệp vụ chính
*   **Memory Cache Key:** Sử dụng một khóa duy nhất `"Services_All_Cache"` để lưu trữ toàn bộ danh sách dịch vụ hợp lệ.
*   **Thời gian lưu đệm:** Thiết lập Absolute Expiration là 60 phút và Sliding Expiration là 15 phút (tự động gia hạn nếu có người truy cập liên tục).
*   **Lọc dữ liệu tại Bộ nhớ:** Thay vì truy vấn cơ sở dữ liệu mỗi khi người dùng gõ từ khóa tìm kiếm, hệ thống lấy mảng thô từ Cache và thực hiện lọc LINQ trên CPU để bảo vệ I/O của PostgreSQL.

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
    public class ServiceService : IServiceService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string ServicesCacheKey = "Services_All_Cache";

        public ServiceService(IApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<ServiceResponseDto>> GetActiveServicesAsync(string? categoryCode, string? searchKeyword)
        {
            // 1. Kiểm tra xem dữ liệu đã có trong Cache chưa
            if (!_cache.TryGetValue(ServicesCacheKey, out List<ServiceResponseDto> cachedServices))
            {
                // 2. Cache Miss: Truy vấn cơ sở dữ liệu PostgreSQL
                cachedServices = await _context.Services
                    .Include(s => s.Category)
                    .Where(s => s.IsActive)
                    .Select(s => new ServiceResponseDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Price = s.BasePrice,
                        ImageUrl = s.ImageUrl,
                        CategoryName = s.Category.Name,
                        CategoryCode = s.Category.Code
                    })
                    .ToListAsync();

                // 3. Cấu hình các tham số lưu trữ Cache
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1)) // Hết hạn tuyệt đối sau 1 giờ
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15)) // Hết hạn sau 15 phút không sử dụng
                    .SetPriority(CacheItemPriority.High);

                // 4. Ghi dữ liệu vào Cache
                _cache.Set(ServicesCacheKey, cachedServices, cacheEntryOptions);
            }

            // 5. Thực hiện Lọc và Tìm kiếm trên CPU Memory (In-Memory Filter)
            var query = cachedServices.AsEnumerable();

            if (!string.IsNullOrEmpty(categoryCode))
            {
                query = query.Where(s => s.CategoryCode.Equals(categoryCode.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                string keyword = searchKeyword.Trim().ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(keyword) || s.Description.ToLower().Contains(keyword));
            }

            return query.ToList();
        }

        /**
         * Xóa bỏ Cache khi quản trị viên cập nhật hoặc thêm dịch vụ mới (Cache Eviction)
         */
        public void ClearServicesCache()
        {
            _cache.Remove(ServicesCacheKey);
        }
    }
}
```

---

## 2. Chiến lược Dọn dẹp Bộ đệm (Cache Eviction Policy)
*   **Vấn đề bất đồng bộ dữ liệu:** Khi admin thay đổi giá dịch vụ tiêm phòng từ 100k lên 150k, trang chủ vẫn hiển thị giá 100k do đang tải từ Cache.
*   **Giải pháp xử lý:** Chúng ta đăng ký cơ chế dọn dẹp chủ động. Mỗi khi API Admin thực hiện các hành động `CreateService`, `UpdateService`, hoặc `DeleteService` thành công:
    *   Gọi hàm `ClearServicesCache()` để xóa Key `"Services_All_Cache"`.
    *   Ở lượt truy cập tiếp theo của khách hàng, hệ thống sẽ gặp trạng thái Cache Miss và tự động nạp lại bảng giá mới từ PostgreSQL, đảm bảo dữ liệu luôn đồng bộ.
