# 📄 User & Dev Documentation - Clinical Diagnosis & Treatment

Tài liệu cung cấp hướng dẫn vận hành chuyên môn dành cho Bác sĩ thú y và cẩm nang tích hợp kỹ thuật chi tiết dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng & Quy trình vận hành (Operator & End-User Guide)

### Quy trình chẩn đoán lâm sàng của Bác sĩ:
1. Đăng nhập tài khoản Bác sĩ, truy cập vào bảng làm việc **"Phòng khám lâm sàng"**.
2. **Hàng chờ khám:** Danh sách thú cưng được lễ tân phân bổ sẽ xếp hàng chờ. Click **"Tiếp nhận khám"** cho ca đầu tiên. Ca khám tự động chuyển sang trạng thái `in_progress`.
3. **Tra cứu y bạ:** Panel bên phải tự động hiển thị Timeline bệnh sử cũ. Bác sĩ click vào từng mốc thời gian để xem chẩn đoán cũ và đơn thuốc cũ của bé.
4. **Ghi nhận khám:** Nhập triệu chứng lâm sàng và chẩn đoán bệnh chính xác.
5. **Kê đơn thuốc:** 
   * Gõ tên thuốc vào ô tìm kiếm (Autocomplete sẽ tự động hiển thị gợi ý sau 2 ký tự).
   * Nhấn Enter hoặc click chọn thuốc còn hàng trong dropdown gợi ý.
   * Nhập số lượng và ghi hướng dẫn liều dùng. Hệ thống sẽ tự động cập nhật tổng chi phí thuốc tạm tính.
6. **Hoàn tất khám:** Bấm **"Hoàn thành ca khám"**. Đơn thuốc và bệnh án được lưu trữ an toàn, tồn kho dược tự động cập nhật và hóa đơn nháp được chuyển sang quầy thu ngân.
7. Đơn thuốc PDF tự động bật lên màn hình. Bác sĩ nhấn in đơn đưa cho chủ nuôi mang ra quầy thuốc thanh toán & nhận thuốc.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
* **Backend .NET Core App:**
  * [MedicalRecordService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/MedicalRecordService.cs) — Logic xử lý lưu trữ bệnh án, trừ kho thuốc bi quan, và tạo hóa đơn nháp.
  * `MyPetClinic.WebApi/Controllers/DoctorClinicalController.cs` — API controller tiếp nhận ghi chép bệnh án và autocomplete thuốc.
  * `MyPetClinic.Domain/Entities/MedicalRecord.cs` — Entity định nghĩa bảng bệnh án lâm sàng.
* **Frontend Vue 3 SPA:**
  * `frontend/src/stores/doctorSession.ts` — Pinia Store quản lý phiên khám và lưu bản nháp LocalStorage.
  * `frontend/src/views/doctor/ClinicWorkspace.vue` — Component UI giao diện làm việc 3 cột của Bác sĩ.

### B. Cấu hình Debounce Autocomplete tìm thuốc (Client-side Performance)
Để tránh spam API gửi hàng chục request liên tiếp khi bác sĩ gõ phím nhanh, tại Component Vue sử dụng kỹ thuật Debounce:

```typescript
import { ref, watch } from 'vue';
import { useDoctorSessionStore } from '@/stores/doctorSession';
import debounce from 'lodash.debounce';

const store = useDoctorSessionStore();
const searchQuery = ref('');

// Cấu hình Debounce 250ms
const performSearch = debounce(async (query: string) => {
  if (query.trim().length >= 2) {
    await store.searchMedicines(query);
  } else {
    store.searchResults = [];
  }
}, 250);

watch(searchQuery, (newVal) => {
  performSearch(newVal);
});
```

### C. Kiểm thử API bằng Curl
Các nhà phát triển sử dụng các lệnh curl sau để kiểm thử nhanh API tại Terminal (yêu cầu gửi kèm Token JWT Doctor hoặc Admin hợp lệ):

#### 1. Tra cứu bệnh sử của thú cưng
```bash
curl -X GET "https://localhost:5001/api/doctor/pets/12/medical-history" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_DOCTOR_TẠI_ĐÂY>"
```

#### 2. Kê đơn & Lưu bệnh án (Gửi request POST)
```bash
curl -X POST "https://localhost:5001/api/doctor/medical-records" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_DOCTOR_TẠI_ĐÂY>" \
     -d "{\"appointmentId\":147,\"petId\":12,\"diagnosis\":\"Viêm dạ dày ruột cấp tính\",\"treatmentPlan\":\"Uống nhiều nước ấm\",\"prescriptionItems\":[{\"medicineId\":16,\"quantity\":10,\"dosageInstructions\":\"Ngày 2 viên\"}]}"
```

---

## 3. Khắc phục sự cố thường gặp (Troubleshooting)

### Sự cố 1: Lỗi Deadlock CSDL khi nhiều bác sĩ cùng lúc lưu bệnh án (PostgreSQL Locks)
* **Nguyên nhân:** Xảy ra khi Bác sĩ A khóa Thuốc X trước rồi yêu cầu khóa Thuốc Y, trong khi Bác sĩ B khóa Thuốc Y trước rồi yêu cầu khóa Thuốc X (Cross-locking). PostgreSQL phát hiện deadlock và tự động giết 1 trong 2 transaction.
* **Khắc phục:** Sắp xếp mảng `PrescriptionItems` tăng dần theo `MedicineId` trước khi thực hiện khóa dòng `FOR UPDATE` trong code C#. Việc khóa tài nguyên theo một thứ tự nhất quán duy nhất loại bỏ hoàn toàn khả năng xảy ra Deadlock:
  ```csharp
  var sortedItems = dto.PrescriptionItems.OrderBy(i => i.MedicineId).ToList();
  ```

### Sự cố 2: Lệch giá thuốc trên hóa đơn nháp
* **Nguyên nhân:** Giá thuốc lấy từ cache của Client gửi lên bị cũ so với giá thuốc thực tế thay đổi dưới DB.
* **Khắc phục:** Tuyệt đối không lấy đơn giá thuốc từ Client gửi lên. Khi tính toán hóa đơn nháp ở Backend, bắt buộc phải truy vấn đơn giá trực tiếp từ bảng `Medicines` dưới cơ sở dữ liệu (`medicine.Price`) để đảm bảo số tiền hóa đơn chính xác 100%.
