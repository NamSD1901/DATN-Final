# 04. Infrastructure & Security - Admin Revenue Reports

Tài liệu thiết kế hạ tầng, an ninh thông tin, giải pháp bộ đệm (Caching) tối ưu hiệu năng và giới hạn tần suất truy vấn báo cáo.

---

## 1. Phân quyền và Bảo mật Dữ liệu tài chính nhạy cảm

Doanh số kinh doanh, hiệu suất làm việc bác sĩ và thông tin thanh toán chi tiết là các dữ liệu tối mật của phòng khám. Hệ thống bắt buộc phải chặn đứng mọi hành vi xem báo cáo trái phép:
- **Áp dụng phân quyền Admin tuyệt đối:** Tất cả các endpoint trong `ReportsController` bắt buộc phải áp dụng bộ lọc `[Authorize(Roles = "admin")]`.
- **Chặn đứng mọi vai trò khác:** Nếu một Bác sĩ (`doctor`) hoặc Lễ tân (`receptionist`) cố gắng gọi các API này bằng cách thay đổi URL, hệ thống Middleware JWT sẽ phát hiện và ngay lập tức chặn lại với mã phản hồi `403 Forbidden` trước khi truy vấn SQL được thực thi.

---

## 2. Giải pháp Bộ đệm Giảm tải Cơ sở dữ liệu (Memory Caching Strategy)

Các truy vấn báo cáo tài chính sử dụng các phép tính tổng hợp (`SUM`, `COUNT`, `AVG`) trên hàng ngàn dòng hóa đơn và chi tiết hóa đơn. Nếu Admin thay đổi bộ lọc liên tục hoặc có nhiều Admin cùng xem Dashboard báo cáo cùng lúc, database sẽ bị quá tải dẫn đến chậm toàn bộ hệ thống khám bệnh.

### Cơ chế Memory Cache (Bộ đệm RAM ngắn hạn):
Hệ thống sử dụng bộ nhớ đệm `IMemoryCache` của ASP.NET Core để lưu trữ kết quả báo cáo trong vòng **15 phút**. 

- **Khóa Cache (Cache Key Generator):** Khóa được sinh tự động dựa trên khoảng ngày lọc và loại API báo cáo. Ví dụ: `Report_KPIs_20260501_20260531`.
- **Thời hạn lưu cache (Sliding Expiration):** 15 phút. Nếu trong 15 phút đó có yêu cầu trùng khớp khoảng ngày lọc, API sẽ trả về kết quả ngay lập tức từ RAM mà không cần truy vấn SQL xuống database.

#### Mã nguồn C# Cài đặt Memory Cache tại API Controller:
```csharp
[HttpGet("kpis")]
[Authorize(Roles = "admin")]
public async Task<IActionResult> GetKpis([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
{
    string cacheKey = $"Report_KPIs_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

    if (!_memoryCache.TryGetValue(cacheKey, out KpiReportDto? cachedKpis))
    {
        // 1. Nếu chưa có cache -> Thực hiện truy vấn DB tính toán
        cachedKpis = await _reportService.GetKpiReportAsync(startDate, endDate);

        // 2. Thiết lập chính sách lưu cache 15 phút
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

        _memoryCache.Set(cacheKey, cachedKpis, cacheEntryOptions);
        
        _logger.LogInformation($"Cache MISSED cho khóa {cacheKey}. Đã truy vấn CSDL.");
    }
    else
    {
        _logger.LogInformation($"Cache HIT cho khóa {cacheKey}. Trả về dữ liệu từ RAM.");
    }

    return Ok(cachedKpis);
}
```

---

## 3. Rate Limiting Chính sách Giới hạn Tần suất Yêu cầu

Nhằm ngăn chặn hành vi tấn công từ chối dịch vụ (DDoS) bằng cách liên tục gọi API báo cáo tài chính nặng nề:
- **API Báo cáo thông thường:** Giới hạn tối đa **15 requests / phút** trên một tài khoản Admin.
- **API Xuất file Excel/PDF:** Giới hạn tối đa **5 requests / phút** trên một tài khoản Admin để tránh quá tải CPU lúc sinh tệp Excel nhị phân.
- **Kích thước bộ lọc ngày tối đa:** Hệ thống chặn các yêu cầu lọc ngày có khoảng cách lớn hơn **3 năm** để tránh làm cạn kiệt tài nguyên máy chủ.
