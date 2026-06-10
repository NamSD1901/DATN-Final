# 🟠 Level 3: QA Advanced (Sprint 4-6)

Cấp độ Nâng cao đòi hỏi năng lực kiểm thử hiệu năng (Performance Testing), thiết lập và quản lý lỗi chuyên nghiệp suốt vòng đời của lỗi, và lập báo cáo chất lượng Sprint.

---

## 🎯 QA-14: Performance Testing (k6)

QA sử dụng công cụ **k6** để viết script giả lập tải nhiều người dùng đồng thời, đánh giá giới hạn chịu tải của APIs Backend .NET 8.

### 💻 Mã Kịch Bản Load Test Mẫu Bằng k6

```javascript
import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Định nghĩa chỉ số đo lường tùy chỉnh
const errorRate = new Rate('errors');
const bookingDuration = new Trend('booking_duration');

// Cấu hình các giai đoạn nâng tải (Stages)
export const options = {
    stages: [
        { duration: '1m', target: 10 },   // Ramp-up: 10 users hoạt động đồng thời trong 1 phút
        { duration: '3m', target: 50 },   // Giữ tải ổn định ở mức 50 users trong 3 phút
        { duration: '1m', target: 100 },  // Đẩy tải lên 100 users trong 1 phút (Stress test)
        { duration: '3m', target: 100 },  // Duy trì 100 users trong 3 phút
        { duration: '1m', target: 0 },    // Ramp-down: hạ tải về 0 trong 1 phút
    ],
    thresholds: {
        http_req_duration: ['p(95)<2000'], // 95% số request phải hoàn thành dưới 2 giây
        errors: ['rate<0.05'],              // Tỉ lệ lỗi phải dưới 5%
    },
};

const BASE_URL = 'https://staging-api.mypetclinic.com/api/v1';

// Setup: Đăng nhập 1 lần ban đầu để lấy token
export function setup() {
    const res = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
        email: 'test_customer@petclinic.com',
        password: 'SecurePassword123'
    }), { headers: { 'Content-Type': 'application/json' } });
    
    return { token: res.json().data.accessToken };
}

// Hàm thực thi chính cho mỗi ảo hóa người dùng (VU)
export default function(data) {
    const headers = {
        'Authorization': `Bearer ${data.token}`,
        'Content-Type': 'application/json'
    };
    
    group('Tải trang dịch vụ phòng khám', () => {
        const res = http.get(`${BASE_URL}/services`, { headers });
        check(res, {
            'status is 200': (r) => r.status === 200,
            'response time < 500ms': (r) => r.timings.duration < 500
        });
        errorRate.add(res.status !== 200);
    });

    group('Kiểm tra lịch trống khả dụng', () => {
        const today = new Date().toISOString().split('T')[0];
        const res = http.get(`${BASE_URL}/slots?date=${today}`, { headers });
        check(res, {
            'status is 200': (r) => r.status === 200,
        });
        errorRate.add(res.status !== 200);
    });

    // Nghỉ ngẫu nhiên 1-3 giây để mô phỏng hành vi người dùng thật
    sleep(Math.random() * 2 + 1);
}
```

---

## 🎯 QA-04: Bug Lifecycle & Reporting

### 1. Vòng Đời Của Lỗi (Bug Lifecycle)
```
[NEW] (QA phát hiện) ──> [ASSIGNED] (PO giao cho Dev) ──> [IN PROGRESS] (Dev sửa)
                            │                                   │
                            v                                   v
[CLOSED] <── [VERIFIED] (QA xác minh đạt) <── [RESOLVED] (Dev sửa xong & deploy)
                 │
                 └──> (QA xác minh lỗi vẫn còn) ──> [REOPENED] ──> [IN PROGRESS]
```

### 2. Mẫu Báo Cáo Lỗi Chuyên Nghiệp (Bug Report Example)
* **Bug ID:** `BUG-042`
* **Mức độ nghiêm trọng (Severity):** 🔴 Critical
* **Độ ưu tiên (Priority):** 🔴 High
* **Môi trường test:** Staging - Chrome v120.
* **Tiêu đề:** `[PB09] Lỗi 500 khi đặt lịch khám với tùy chọn "Bác sĩ bất kỳ"`
* **Các bước tái hiện (Steps to Reproduce):**
  1. Đăng nhập tài khoản: `customer@petclinic.com` / `Password123`
  2. Vào trang `/booking`. Chọn pet "Milo", chọn dịch vụ "Khám định kỳ", ngày 15/12/2026.
  3. Tại trường "Chọn bác sĩ", chọn "Bác sĩ bất kỳ" (Any Vet). Nhấn "Xác nhận đặt lịch".
* **Kết quả thực tế (Actual Result):** API trả về mã lỗi `500 Internal Server Error`, hiển thị Toast "Có lỗi xảy ra". Không có lịch hẹn nào được tạo trong DB.
* **Console Logs / Stack Trace:**
  ```csharp
  System.NullReferenceException: Object reference not set to an instance of an object.
     at MyPetClinic.Application.Services.BookingService.CreateBooking(BookingDto dto)
  ```
* **Gợi ý cách khắc phục (Suggested Fix):** 
  Backend nhận `vetId = null` nhưng không xử lý case này. Cần bổ sung logic: nếu `vetId == null`, hệ thống tự động gọi hàm tìm kiếm và gán ID bác sĩ trống lịch khả dụng gần nhất vào lịch hẹn trước khi lưu xuống Database.

---

## 🎯 QA-17: Test Metrics & Reporting

Cuối mỗi Sprint, QA Lead có nhiệm vụ lập Báo cáo chất lượng (Sprint Quality Report) để đánh giá mức độ sẵn sàng bàn giao của hệ thống.

### 📋 Mẫu Báo Cáo Chất Lượng Sprint (Sprint Quality Report Template)
* **Tổng số lượng test cases dự kiến:** 85 | **Đã thực thi:** 85 (100%).
* **Số ca test Đạt (Passed):** 78 (91.8%) | **Lỗi (Failed):** 7 (8.2%).
* **Thống kê lỗi theo độ nghiêm trọng:** Critical: 0 | High: 1 | Medium: 3 | Low: 3.
* **Các chỉ số đo lường (Quality Metrics):**
  - *Mật độ lỗi (Defect Density):* 0.8 lỗi / 100 dòng code. (Target: $< 1.0$) -> **Đạt**.
  - *Tỷ lệ lọt lỗi (Defect Escape Rate):* 15% (Số lỗi người dùng phát hiện ở Staging/UAT sau khi QA đã ký nghiệm thu ở Dev). (Target: $< 10\%$) -> **Cần cải tiến**.
  - *Thời gian sửa lỗi trung bình (MTTR):* 4.2 giờ. (Target: $< 8$ giờ) -> **Tốt**.
* **Xác nhận bàn giao (Sign-off Criteria):**
  - [x] Không còn lỗi thuộc nhóm Critical và High chưa được sửa.
  - [x] 95% kịch bản kiểm thử lõi ở trạng thái Passed.
  - [x] Tất cả Acceptance Criteria của các User Stories được đáp ứng đầy đủ.
  - [x] PO và QA Lead cùng ký biên bản xác nhận đóng Sprint.
