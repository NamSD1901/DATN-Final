
## [24/06/2026] L?i d?t l?ch khám cho h? so Walk-in du?c d?ng b?
- **Tr?ng thái:** Ðã kh?c ph?c (? FIXED)
- **Tri?u ch?ng:** Ngu?i dùng d?ng b? h? so Walk-in g?p l?i không d?t l?ch du?c, API báo l?i 400 Bad Request ho?c An error occurred while saving the entity changes (Foreign key constraint violation). Ngoài ra còn dính l?i l?ch múi gi? (d?t tru?c 2 ti?ng).
- **Nguyên nhân c?t lõi:**
  1. B? l?i l?ch múi gi? UTC và Local khi ki?m tra ràng bu?c th?i gian (dã kh?c ph?c b?ng DateTime.Now).
  2. B? vi ph?m ràng bu?c khóa ngo?i (Foreign Key) ? b?ng ppointments c?t created_by -> users.id. Service dã truy?n nh?m customerId vào v? trí c?a createdBy, d?n t?i database báo l?i khi c? g?ng luu l?ch h?n.
- **Gi?i pháp:**
  1. S?a l?i timezone s? d?ng DateTime.Now thay vì DateTime.UtcNow. Gi?m th?i gian d?t l?ch t?i thi?u t? 2 ti?ng xu?ng 15 phút theo yêu c?u.
  2. S?a ch? ký hàm BookAppointmentAsync d? ti?p nh?n userId l?y t? JWT Claims và truy?n chính xác vào tru?ng CreatedBy khi t?o Appointment, d?m b?o tính toàn v?n d? li?u khóa ngo?i.

<<<<<<< Updated upstream
- **Tráº¡ng thÃ¡i:** FIXED
- **Thá»i gian:** 22-06-2026

### NguyÃªn nhÃ¢n
1. Backend MedicalRecordService.cs (GetPetMedicalHistoryAsync) Ä‘ang truy váº¥n .Appointment.PetId == petId thay vÃ¬ truy váº¥n trá»±c tiáº¿p vÃ o .PetId == petId. Äiá»u nÃ y cÃ³ thá»ƒ dáº«n Ä‘áº¿n khÃ´ng tráº£ vá» báº£n ghi nÃ o náº¿u Appointment bá»‹ detach.
2. Frontend Vue Component ConsultationRecordTab.vue Ä‘ang tham chiáº¿u cÃ¡c trÆ°á»ng khÃ´ng tá»“n táº¡i trÃªn MedicalRecordDto (vÃ­ dá»¥: ecord.symptoms thay vÃ¬ ecord.clinicalSigns, ecord.treatment thay vÃ¬ ecord.treatmentPlan, ecord.note thay vÃ¬ ecord.doctorNotes). DTO cÅ©ng tráº£ vá» máº£ng object thuá»‘c prescribedMedicines chá»© khÃ´ng pháº£i máº£ng chuá»—i.

### Giáº£i phÃ¡p
1. Sá»­a LINQ query trong backend thÃ nh .PetId == petId Ä‘á»ƒ láº¥y trá»±c tiáº¿p há»“ sÆ¡ bá»‡nh Ã¡n theo PetId.
2. Sá»­a láº¡i cÃ¡c property bindings trong template Vue ConsultationRecordTab.vue Ä‘á»ƒ khá»›p chÃ­nh xÃ¡c vá»›i DTO tráº£ vá», bao gá»“m cáº£ máº£ng object thuá»‘c.

## [BUG-PET-003] Root Cause XÃ¡c Ä‘á»‹nh: petId=0 trong localStorage

- **Tráº¡ng thÃ¡i:** FIXED
- **Thá»i gian:** 22-06-2026

### NguyÃªn nhÃ¢n gá»‘c rá»…
Backend AppointmentService.GetCalendarEventsAsync() xÃ¢y dá»±ng ExtendedProps cho má»—i sá»± kiá»‡n lá»‹ch nhÆ°ng **thiáº¿u trÆ°á»ng petId vÃ  customerId**. Frontend DoctorQueueTab.vue khi bÃ¡c sÄ© nháº¥n 'Tiáº¿n hÃ nh khÃ¡m' Ä‘á»c evt.extendedProps?.petId â†’ tráº£ vá» undefined â†’ fallback || '0' â†’ lÆ°u chuá»—i "0" vÃ o localStorage. Khi ConsultationRecordTab mount lÃªn, gá»i API /medical-records/pet/0 â†’ khÃ´ng cÃ³ báº£n ghi nÃ o.

### Giáº£i phÃ¡p
1. **Backend AppointmentService.cs**: ThÃªm petId = a.PetId vÃ  customerId = a.CustomerId vÃ o object ExtendedProps trong GetCalendarEventsAsync().
2. **Backend DoctorController.cs**: ThÃªm endpoint GET /doctor/appointment/{appointmentId} cho phÃ©p bÃ¡c sÄ© láº¥y thÃ´ng tin cuá»™c háº¹n (Ä‘á»ƒ fallback resolve petId khi giÃ¡ trá»‹ cÅ© trong localStorage báº±ng 0).
3. **Frontend ConsultationRecordTab.vue**: ThÃªm logic kiá»ƒm tra petId > 0 trÆ°á»›c khi gá»i etchPetHistory(). Náº¿u petId = 0, tá»± Ä‘á»™ng gá»i fallback API /doctor/appointment/{id} Ä‘á»ƒ láº¥y petId thá»±c vÃ  tá»± sá»­a localStorage.

## [BUG-APPT-004] Lá»—i khung giá» trá»‘ng khÃ´ng Ä‘á»“ng bá»™ vá»›i cáº¥u hÃ¬nh Operating Hours

- **Tráº¡ng thÃ¡i:** FIXED
- **Thá»i gian:** 22-06-2026

### NguyÃªn nhÃ¢n gá»‘c rá»…
Endpoint láº¥y cÃ¡c khung giá» kháº£ dá»¥ng (GET /api/appointments/available-slots) vÃ  logic táº¡o lá»‹ch háº¹n (CreateAppointmentAsync) hoÃ n toÃ n bá» qua thiáº¿t láº­p **Khung giá» hoáº¡t Ä‘á»™ng chung** (ClinicOperatingDays / ClinicOperatingShifts) vÃ  **NgÃ y nghá»‰ lá»…** (ClinicHolidays). Cáº£ 2 Ä‘ang fallback vá» cáº¥u hÃ¬nh lá»‹ch trá»±c cá»§a bÃ¡c sÄ© (DoctorSchedule) hoáº·c sinh tá»± Ä‘á»™ng giá» hÃ nh chÃ­nh tá»« slot_config.json (tá»« 8h-20h), do Ä‘Ã³ bÃ¡c sÄ© cÃ³ thá»ƒ cÃ³ khung giá» trá»‘ng vÃ  khÃ¡ch hÃ ng váº«n Ä‘áº·t lá»‹ch Ä‘Æ°á»£c vÃ o cÃ¡c khoáº£ng thá»i gian mÃ  phÃ²ng khÃ¡m Ä‘Ã¡ng láº½ Ä‘Ã£ Ä‘Ã³ng cá»­a.

### Giáº£i phÃ¡p
1. **Trong GetAvailableSlotsAsync (AppointmentService.cs)**:
   - Truy váº¥n ClinicHolidays tÆ°Æ¡ng á»©ng vá»›i ngÃ y háº¹n. Náº¿u lÃ  ngÃ y lá»…, láº­p tá»©c tráº£ vá» máº£ng rá»—ng [] (khÃ´ng cÃ³ bÃ¡c sÄ© nÃ o nháº­n khÃ¡m).
   - Truy váº¥n ClinicOperatingDays. Náº¿u ngÃ y Ä‘Ã³ cáº¥u hÃ¬nh IsOpen = false, láº­p tá»©c tráº£ vá» [].
   - Lá»c cÃ¡c má»‘c thá»i gian kháº£ dá»¥ng (do SlotCalculationHelper sinh ra) báº±ng cÃ¡ch Ä‘á»‘i chiáº¿u vá»›i danh sÃ¡ch cÃ¡c ClinicOperatingShifts cá»§a ngÃ y Ä‘Ã³. CÃ¡c khung giá» nÃ o náº±m ngoÃ i hoáº·c trÃ n ra khá»i giá» hoáº¡t Ä‘á»™ng sáº½ bá»‹ loáº¡i bá» ngay tá»« phÃ­a Server.
   
2. **Trong CreateAppointmentAsync (AppointmentService.cs)**:
   - ThÃªm bÆ°á»›c xÃ¡c thá»±c Ä‘áº§u vÃ o (Validation) trÆ°á»›c khi láº¥y bÃ¡c sÄ© vÃ  phÃ¢n lá»‹ch:
     - Náº¿u ngÃ y háº¹n trÃ¹ng ngÃ y lá»… IsActive, nÃ©m ra InvalidOperationException("PhÃ²ng khÃ¡m Ä‘Ã³ng cá»­a vÃ o ngÃ y nghá»‰ lá»… nÃ y...").
     - Náº¿u cáº¥u hÃ¬nh phÃ²ng khÃ¡m trong ngÃ y khÃ´ng hoáº¡t Ä‘á»™ng (!IsOpen), nÃ©m ra lá»—i.
     - Kiá»ƒm tra trá»±c tiáº¿p thá»i gian háº¹n (AppointmentDate.TimeOfDay) vá»›i cÃ¡c ca trá»±c cá»§a phÃ²ng khÃ¡m. Náº¿u thá»i gian náº±m ngoÃ i má»i ca hoáº·c thá»i lÆ°á»£ng khÃ¡m trÃ n ra khá»i giá» nghá»‰ ca, cháº·n viá»‡c Ä‘áº·t lá»‹ch.

## [BUG-APPT-005] NgÃ y háº¹n hiá»ƒn thá»‹ sai lá»‡ch khi Ä‘áº·t qua giao diá»‡n khÃ¡ch hÃ ng

- **Tráº¡ng thÃ¡i:** FIXED
- **Thá»i gian:** 24-06-2026

### NguyÃªn nhÃ¢n gá»‘c rá»…
Frontend gá»­i AppointmentDate dáº¡ng yyyy-MM-ddTHH:mm:00 (VD: 25/06/2026 10:30), Ä‘Æ°á»£c Backend deserialize vá»›i Kind=Unspecified. Khi EF Core lÆ°u vÃ o PostgreSQL, DateTimeUtcConverter gá»i .ToUniversalTime() chuyá»ƒn giá» Local (Vietnam +07:00) thÃ nh giá» UTC (VD: 24/06/2026 17:30 UTC). Khi Backend truy váº¥n vÃ  map vÃ o AppointmentDetailDto, viá»‡c gá»i .Date trÃªn giÃ¡ trá»‹ UTC nÃ y tráº£ vá» ngÃ y 24 thay vÃ¬ 25, dáº«n Ä‘áº¿n lá»—i lá»‡ch ngÃ y hiá»ƒn thá»‹ trÃªn giao diá»‡n ngÆ°á»i dÃ¹ng.

### Giáº£i phÃ¡p
1. **Trong AppointmentService.cs**: Cáº­p nháº­t táº¥t cáº£ cÃ¡c biá»ƒu thá»©c mapping DTO tá»« .AppointmentDate.Date.Add(a.StartTime) thÃ nh .AppointmentDate.ToLocalTime().Date.Add(a.StartTime). Viá»‡c chuyá»ƒn Ä‘á»•i vá» LocalTime trÆ°á»›c khi láº¥y Date giÃºp láº¥y láº¡i Ä‘Ãºng mÃºi giá» trÆ°á»›c khi ná»‘i chuá»—i ngÃ y thÃ¡ng gá»­i vá» Frontend.
2. **Trong SlotCalculationHelper.cs**: Cáº­p nháº­t biá»ƒu thá»©c tÃ­nh pptTime tÆ°Æ¡ng tá»± Ä‘á»ƒ logic kiá»ƒm tra trÃ¹ng lá»‹ch khÃ´ng bá»‹ sai lá»‡ch ngÃ y.

## [BUG-WALKIN-001] Lá»—i 500 khi táº¡o lá»‹ch háº¹n vÃ£ng lai do truy váº¥n LINQ
- **Tráº¡ng thÃ¡i:** FIXED
- **Thá»i gian:** 28-06-2026
### NguyÃªn nhÃ¢n
DÃ¹ng .Contains("doctor") trÃªn Ä‘á»‘i tÆ°á»£ng Role gÃ¢y lá»—i dá»‹ch ngÆ°á»£c (InvalidOperationException) cá»§a EF Core trÃªn PostgreSQL.
### Giáº£i phÃ¡p
Sá»­a láº¡i truy váº¥n so sÃ¡nh chuá»—i tÆ°á»ng minh: .Where(u => u.Role != null && (u.Role.Name.ToLower() == "clinical_doctor" || u.Role.Name.ToLower() == "vaccination_doctor" || u.Role.Name.ToLower() == "doctor") && u.IsActive == true && u.DeletedAt == null).

## [BUG-WALKIN-002] Lá»—i 400 Bad Request thiáº¿u Sá»‘ Ä‘iá»‡n thoáº¡i
- **Tráº¡ng thÃ¡i:** FIXED
- **Thá»i gian:** 28-06-2026
### NguyÃªn nhÃ¢n
API GetCustomerByPhone khÃ´ng tráº£ vá» sá»‘ Ä‘iá»‡n thoáº¡i. Frontend láº¥y selectedCustomer.value.phone bá»‹ undefined, dáº«n Ä‘áº¿n Model Validation [Required] cá»§a WalkInRequestDto.Phone bá»‹ lá»—i 400 Bad Request, tráº£ vá» thÃ´ng bÃ¡o chung chung.
### Giáº£i phÃ¡p
1. ThÃªm Phone vÃ o object tráº£ vá» cá»§a API GetCustomerByPhone trong ReceptionistController.cs.
2. Bá»c lÃ³t láº¥y customerForm.value.customerPhone trong AppointmentsTab.vue náº¿u phone rá»—ng. ThÃªm Validation frontend cho serviceId vÃ  hiá»ƒn thá»‹ chi tiáº¿t máº£ng errors.

## [BUG-WALKIN-003] Lá»—i lá»‡ch mÃºi giá» khi lÆ°u giá» háº¹n vÃ£ng lai
- **Tráº¡ng thÃ¡i:** FIXED
- **Thá»i gian:** 28-06-2026
### NguyÃªn nhÃ¢n
CreateWalkInAsync sá»­ dá»¥ng DateTime.UtcNow Ä‘á»ƒ lÆ°u AppointmentDate, StartTime, CheckInTime. Náº¿u giá» Viá»‡t Nam lÃ  19h27, giá» UTC lÃ  12h27, CSDL lÆ°u 12h27 dáº«n Ä‘áº¿n giao diá»‡n hiá»ƒn thá»‹ sai.
### Giáº£i phÃ¡p
DÃ¹ng TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTimeZone) Ä‘á»ƒ chuyá»ƒn sang giá» Viá»‡t Nam trÆ°á»›c khi gÃ¡n vÃ o cÃ¡c thuá»™c tÃ­nh thá»i gian.
## [BUG-QUEUE-001] Trang thai lich kham bi bo qua buoc Cho thanh toan
- **Trang thai:** FIXED
- **Thoi gian:** 30-06-2026
### Nguyen nhan
Khi bac si hoan tat kham va tao MedicalRecord, MedicalRecordService.cs gan truc tiep appointment.Status = "completed" thay vi "ready_to_pay". Do do, ca kham bi lot qua buoc hien thi tren bang Hang kham voi cot "Cho thanh toan".
### Giai phap
Sua doi appointment.Status = "ready_to_pay" trong MedicalRecordService.cs khi khoi tao benh an moi qua SOAP hoac thong thuong.

| 6/30/2026 | BUG-MED-001 | Medicine stock desync causes MedicalRecord save failure | ExportMedicineAsync deducted Batch CurrentQuantity but forgot Medicine StockQuantity | Fixed in MedicineService.cs |

| L?i bi?n m?t l?ch h?n cu | Entity Framework Core INNER JOIN v?i các b?n ghi liên quan (bác si, thú cung) b? soft-delete (xóa m?m), khi?n truy v?n Select vô tình lo?i b? l?ch h?n trong danh sách tr? v?. Count v?n d?m d? nhung khi Skip().Take() thì k?t qu? b? h?t. | Thêm .IgnoreQueryFilters() vào truy v?n LINQ t?i AppointmentService.cs d? b? qua b? l?c xóa m?m c?a b?ng Users và Pets. |

| L?i phân trang b? tr?ng (?n nút) do d? li?u b? orphaned | Vi?c dùng .IgnoreQueryFilters() chua d? n?u b?n ghi (Pet, Doctor) b? xóa c?ng (hard-delete), EF Core v?n dùng INNER JOIN lo?i b? l?ch h?n. Gi?i pháp: L?y danh sách l?ch h?n tru?c b?ng ToList() (ch? 5 record m?i trang) r?i gán d? li?u th? công (manual fetching). Frontend cung c?n chuy?n nút phân trang ra ngoài -else d? không b? ?n. | Vi?t hàm MapToDetailDtoAsync trong AppointmentService.cs d? query d? li?u r?i r?c, tránh EF Core t?o INNER JOIN. |
=======

### Error: Follow-up Appointment Timezone & Missing Slots Bug
- **Symptom:** Bác sĩ chọn lịch tái khám nhưng danh sách báo rỗng, hoặc chọn giờ 08:00 nhưng khách hàng xem lại thấy 02:00.
- **Root Cause:** 
  1. Lỗi EF Core dịch sai biểu thức !allowedDoctorEmails.Any() khi mảng rỗng.
  2. Bác sĩ gọi nhầm endpoint /my-appointments/available-slots của Khách hàng, dẫn đến lỗi 403.
  3. Frontend dùng hàm .toISOString() để gửi giờ làm nó bị lệch múi giờ (UTC lùi 7 tiếng).
- **Solution:**
  1. Sửa biểu thức LINQ trong AppointmentService.cs (tách riêng biến ool filterByEmail).
  2. Chuyển Frontend gọi API /appointment/available-slots và cấp quyền truy cập cho clinical_doctor.
  3. Xóa lệnh .toISOString() bên Frontend để gửi giờ dạng Local.
- **Status:** Resolved (Đã push lên nhánh fix/follow-up-timezone-and-fallback)

### Error: Race condition and invalid double-booking logic in Appointment Rescheduling
- **Symptom:** Lễ tân dời lịch hoặc đổi bác sĩ thì lọt qua kiểm tra trùng giờ, khách khác cùng lúc đặt lịch cũng lọt qua dẫn tới 2 người cùng 1 slot.
- **Root Cause:** 
  1. AppointmentDate trong DB chỉ lưu Ngày, nhưng logic cũ ở RescheduleAppointmentAsync so sánh AppointmentDate > targetDate.AddMinutes(-30) (có lẫn giờ) dẫn tới luôn False.
  2. Cả RescheduleAppointmentAsync và UpdateAppointmentDoctorAsync đều không bọc Transaction Serializable dẫn tới lọt qua khi Race Condition.
- **Solution:** 
  1. Cập nhật logic so sánh AppointmentDate == targetDate.Date và sau đó so sánh StartTime.
  2. Bọc toàn bộ block code bằng _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable) kèm cơ chế retry loop và IsTransientConflict(ex).
- **Status:** Resolved

### Error: Double-booking allowed during Appointment Creation (Race condition bypass)
- **Symptom:** Khách hàng đặt lịch song song hoặc lễ tân đặt liên tục cùng 1 lúc có thể khiến 2 người vào cùng 1 bác sĩ ở cùng 1 giờ.
- **Root Cause:** 
  1. Lỗi dịch múi giờ ở \ResolveAndValidateDoctorId\: biến \ppointmentDate.Date\ mang kind \Unspecified\, khi EF Core so sánh với \AppointmentDate\ (timestamp with time zone) trong CSDL sẽ bị lùi 7 tiếng (VD 00:00 ngày 11/7 thành 17:00 ngày 10/7). Do đó, hàm check trùng lịch trả về 0 kết quả.
  1. Lỗi dịch múi giờ ở \ResolveAndValidateDoctorId\: biến \ ppointmentDate.Date\ mang kind \Unspecified\, khi EF Core so sánh với \AppointmentDate\ (timestamp with time zone) trong CSDL sẽ bị lùi 7 tiếng (VD 00:00 ngày 11/7 thành 17:00 ngày 10/7). Do đó, hàm check trùng lịch trả về 0 kết quả.
  2. Trả về 0 kết quả khiến vòng bảo vệ \IsolationLevel.Serializable\ không nhận diện được xung đột Read-Write (SSI) của PostgreSQL, làm cho cả 2 luồng đều lọt qua được và cùng tạo ra 2 lịch mới.
- **Solution:** Sửa lại \	argetDateStart\ bằng \DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc)\ và sử dụng cho toàn bộ các truy vấn LINQ bên trong \ResolveAndValidateDoctorId\.
- **Status:** Resolved

### Error: UI shows 'ĐÃ ĐẶT' (Booked) when only 1 doctor is booked while others are available
- **Symptom:** In the 'Auto assign' mode (Tự động phân công), if one doctor is booked for a slot, the slot shows as fully booked, preventing users from booking with other available doctors.
- **Root Cause:** In \AppointmentService.cs\, the API \GetAvailableSlotsAsync\ was using a hardcoded list of \ llowedDoctorEmails\ (e.g., only bacsituantran@gmail.com) for certain services. This caused the system to completely ignore other active doctors like Bs. Long. When Bs. Tuấn was booked, the slot appeared fully booked because no other doctors were considered.
- **Solution:** Removed the hardcoded \ llowedDoctorEmails\ filtering entirely from \GetAvailableSlotsAsync\ and \ResolveAndValidateDoctorId\. The system now dynamically relies on the doctor's active schedules (\DoctorSchedules\) and roles to determine availability.
- **Status:** Resolved

### Error: Crash and white screen in Doctor Schedules Tab (SchedulesAdminTab.vue)
- **Symptom:** Màn hình quản lý lịch trực bác sĩ bị crash và hiển thị trắng tinh (white screen) khi load hoặc cập nhật dữ liệu.
- **Root Cause:** 
  1. Trong Vue 3, việc gán trực tiếp một mảng động (reactive proxy hoặc computed object) vào `calendarOptions.value.events` với watcher `deep: true` tạo ra vòng lặp vô hạn (infinite loop) khi thư viện FullCalendar cố gắng duyệt và thay đổi (mutate) mảng sự kiện bên trong, gây tràn bộ nhớ stack (stack overflow).
  2. Bỏ sót kiểm tra an toàn (null-check) đối với `workDate`, `startTime`, và `endTime` trong trường hợp dữ liệu bị thiếu.
- **Solution:** 
  1. Tách mảng `events` ra khỏi `calendarOptions` và truyền trực tiếp vào component thông qua prop `:events="calendarEvents"`. Xóa bỏ watcher `deep: true`.
  2. Bổ sung các câu lệnh `if (!s.workDate || !s.startTime || !s.endTime) return;` trước khi thực hiện `.split('T')`.
- **Status:** Resolved

### Error: Missing Sunday Schedules on the User Interface (Timezone/Boundary Bug)
- **Symptom:** Khi táº¡o máº«u lá»‹ch trá»±c (Profile) cÃ³ bao gá»“m ngÃ y Chá»§ Nháº­t (Sunday) vÃ  gÃ¡n cho bÃ¡c sÄ©, giao diá»‡n LÆ°á»›i Thá» i Gian khÃ´ng hiá»ƒn thá»‹ lá»‹ch cá»§a ngÃ y Chá»§ Nháº­t. CÃ¡c ngÃ y tÆ° thá»© 2 Ä‘áº¿n thá»© 7 vÃ¢n hiá»ƒn thá»‹ Ä‘áº§y Ä‘á»§.
- **Root Cause:** 
  1. API `/doctor-schedules?startDate=...&endDate=...` nháº­n tham sá»‘ `endDate` dÆ°á»›i dáº¡ng chuá»—i (vd: 2026-07-19) vÃ  parse thÃ nh `DateTime` vá»›i `Kind = Unspecified`.
  2. BÃªn trong `ApplicationDbContext.cs`, EF Core sá»± dá»¥ng `DateTimeUtcConverter` Ä‘á»ƒ chÆ°yá»ƒn Ä‘á»•i DateTime sang UTC trÆ°á»›c khi truy váº¥n. PhÆ°Æ¡ng thá»©c `.ToUniversalTime()` Ä‘Æ°á»£c Ã¡p dá»¥ng lÃªn `Unspecified DateTime` sáº½ hiá»ƒu ngáº§m Ä‘Ã³ lÃ  Local Time, vÃ  bá»‹ lÃ¹i 7 tiáº¿ng (vÃ­ dá»¥: `2026-07-19 00:00:00` sáº½ biáº¿n thÃ nh `2026-07-18 17:00:00 UTC`).
  3. Giá»›i háº¡n trÃªn (endDate) bá»‹ lÃ¹i láº¡i, nÃªn lÃ m rá»›t máº¥t lá»‹ch cá»§a Chá»§ Nháº­t Ä‘Æ°á»£c lÆ°u nhÆ° lÃ  `2026-07-19 00:00:00 UTC` trong CSDL.
- **Solution:** 
  1. SÆ°a `DoctorScheduleService.cs`: SÆ° dá»¥ng `DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)` vÃ  `DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc)` trÆ°á»›c khi tiáº¿n hÃ nh so sÃ¡nh LINQ. Viá»‡c áº¥n Ä‘á»‹nh rÃµ UTC kind sáº½ ngÄƒn ngá»«a EF Core láº·p láº¡i thao tÃ¡c lÃ¹i giá»  sai lá»‡ch.
- **Status:** Resolved

### Error: Walk-in and Appointment Creation allows wrong doctor role assignment
- **Symptom:** Lễ tân có thể phân công bác sĩ sai chuyên khoa (ví dụ: chọn bác sĩ Khám bệnh cho ca Tiêm phòng).
- **Root Cause:** Logic lấy `targetRole` (như "clinical_doctor" hoặc "vaccination_doctor") dựa trên CategoryName của dịch vụ chỉ được thực thi bên trong khối lệnh `if (finalDoctorId == Guid.Empty)` (khi tự động phân công). Khi lễ tân chọn đích danh bác sĩ (`finalDoctorId != Guid.Empty`), biến `targetRole` không được tính toán, dẫn đến việc bỏ qua bước xác thực role của bác sĩ.
- **Solution:** Đưa logic lấy `targetRole` ra ngoài khối lệnh `if`, sau đó bổ sung bước lấy RoleName của bác sĩ được chọn và kiểm tra sự trùng khớp với `targetRole` trong nhánh `else`. Quăng lỗi `InvalidOperationException` nếu không khớp.
- **Status:** Resolved

### Error: Walk-in appointments do not show up on Doctor's Calendar
- **Symptom:** Các ca khám Walk-in được tạo lẻ giờ (VD: 17:19) không hiển thị trên lưới lịch làm việc của bác sĩ. Lưới chỉ hiện các ca khám Online (được chốt chẵn giờ như 17:00, 17:30).
- **Root Cause:** Ở Frontend (`DoctorQueueTab.vue`), hàm `getEventsForCell` rà soát tuyệt đối giờ của ca khám so với mốc giờ cứng trên table. Giờ 17:19 không khớp với mốc 17:00 hay 17:30 nên bị bỏ qua hoàn toàn.
- **Solution:** Thêm logic vào `getEventsForCell` để làm tròn xuống giờ khám lẻ về mốc 30 phút gần nhất (Ví dụ 17:19 làm tròn về 17:00). Đồng thời tạo hàm `getActualTime` để render giờ thực tế lên trực tiếp trên Card UI hiển thị cho bác sĩ (Ví dụ: `[17:19] Tên thú cưng`).
- **Status:** Resolved

### Error: Walk-in auto assignment picks wrong doctor roles and distributes unfairly
- **Symptom:** Đặt lịch khám walk-in ở lễ tân (khi để trống bác sĩ để hệ thống tự động phân công) phân công sai chuyên khoa (ví dụ dịch vụ Tiêm chủng lại chọn bác sĩ bên Khám bệnh). Hơn nữa, nó chỉ phân công toàn bộ khách cho 1 bác sĩ duy nhất.
- **Root Cause:** 
  1. Trong `ReceptionistService.CreateWalkInAsync`, khi `request.DoctorId` trống, logic query bác sĩ lấy danh sách tất cả bác sĩ đang active mà không lọc theo danh mục của dịch vụ (Service Category).
  2. Bác sĩ được chọn luôn luôn là `doctors.FirstOrDefault()` - người đầu tiên xuất hiện trong kết quả truy vấn Database, dẫn đến dồn hết lịch cho 1 người.
- **Solution:** 
  1. Áp dụng logic lọc tương tự như `AppointmentService`, truy vấn `ServiceCategory` trước để lấy `targetRole` (Khám bệnh -> clinical_doctor, Tiêm phòng -> vaccination_doctor). 
  2. Thay vì dùng `FirstOrDefault()`, query `Appointments` để đếm số lượng ca khám đang chờ/đang xử lý (`waiting`, `in_progress`, `pending`, `ready_to_pay`, `confirmed`) của từng bác sĩ trong ngày. Dùng `.OrderBy(d => count)` để tự động chọn bác sĩ đang rảnh nhất.
- **Status:** Resolved

### Error: Wait time display stuck at 0 minutes in Queue Tab
- **Symptom:** Bộ đếm thời gian chờ của bệnh nhi trên Hàng Khám của Lễ Tân luôn hiển thị "0 phút" dù bệnh nhi đã chờ từ lâu (VD: Check-in từ 19:09 nhưng lúc 19:20 vẫn báo 0 phút).
- **Root Cause:** Trong CSDL, biến `vnTime` (chứa Local Time) được lưu thẳng vào cột `AppointmentDate` và `CheckInTime`. Khi dữ liệu được gọi qua API, EF Core's `DateTimeUtcConverter` mặc định gắn cờ UTC nên trả về định dạng `yyyy-MM-ddTHH:mm:00Z`. Frontend lấy chuỗi này bỏ vào `new Date()` sẽ bị hiểu nhầm là giờ UTC, tự động cộng thêm 7 tiếng. Do đó, `refTime` luôn nằm ở thì tương lai, khiến phép trừ thời gian `nowRef - refTime` bị âm và quy về 0.
- **Solution:** Tạo hàm `fixTimezone` ở `QueueTab.vue` để loại bỏ ký tự `Z` ở đuôi chuỗi thời gian trả về (`timeStr.slice(0, -1)`) trước khi parse `new Date()`. Việc này ép trình duyệt phân tích cú pháp thời gian thành Local Time, giúp bộ đếm hiển thị chính xác số phút đã trôi qua.
- **Status:** Resolved

## [BUG-APPT-006] Lịch khám do lễ tân đặt không hiển thị trên Customer Portal
- **Trạng thái:** FIXED
- **Thời gian:** 17-07-2026
### Nguyên nhân
Khi Lễ tân tìm kiếm số điện thoại khách hàng bằng API `QuickSearch` hoặc tạo lịch hẹn, backend gọi hàm `GetCustomerWithPetsByPhoneAsync` trong `ReceptionistService.cs`. Hàm này trước đây query sai từ bảng `Users` thay vì `Customers`, và trả về `user.Id` thay vì `user.CustomerId`. Do đó, `Appointment` được gán `CustomerId` bằng ID của User. 
Tuy nhiên, khi khách hàng đăng nhập, `CustomerAppointmentController` chỉ trả về lịch hẹn lọc theo `CustomerId` thật của khách hàng, nên lịch hẹn bị gán nhầm ID không hiển thị.
### Giải pháp
Sửa lại hàm `GetCustomerWithPetsByPhoneAsync` để query trực tiếp từ bảng `Customers` (bỏ qua bảng `Users`). Trả về đúng `customer.Id`, đảm bảo Lễ tân gán đúng hồ sơ cho lịch hẹn, và lúc đó Customer Portal sẽ hiển thị được lịch hẹn đồng bộ với Lễ tân.

## [BUG-APPT-007] Thiếu backend validation khi đặt lịch tái khám từ giao diện Bác sĩ
- **Trạng thái:** FIXED
- **Thời gian:** 17-07-2026
### Nguyên nhân
Khi bác sĩ sử dụng `ConsultationRecordTab.vue` hoặc `MedicalRecordsTab.vue` để đặt lịch tái khám, Frontend lấy danh sách tất cả giờ trống của **toàn bộ** bác sĩ thay vì chỉ lọc giờ trống của chính bác sĩ đó. Nguy hiểm hơn, tại phía Backend, `MedicalRecordService` khi gọi `CreateSoapMedicalRecordAsync` đã bỏ qua mọi bước kiểm tra validation (không kiểm tra bác sĩ có lịch trực không, phòng khám có đóng cửa không, có bị trùng 2 lịch cùng lúc không). Điều này dẫn tới việc nếu 2 bệnh nhân cùng lúc đặt lịch tái khám, bác sĩ có thể bị dồn lịch trùng giờ nhau (Double-booking) hoặc lịch rơi vào ngày nghỉ của phòng khám.
### Giải pháp
1. **Phía Frontend:** Cập nhật `ConsultationRecordTab.vue`, bổ sung biến lưu trữ `currentDoctorId` lấy từ thông tin cuộc hẹn đang khám. Khi hiển thị danh sách các khung giờ trống, tiến hành lọc mảng `res.data` chỉ giữ lại các giờ trống thuộc về `doctorId` của bác sĩ đang đăng nhập.
2. **Phía Backend:** Cập nhật `CreateSoapMedicalRecordAsync` và `CreateMedicalRecordAsync` trong `MedicalRecordService.cs`. Đưa toàn bộ module kiểm tra validation của `AppointmentService` (kiểm tra Clinic Holidays, Clinic Operating Shifts, Doctor Schedules, Duplicate Customer/Doctor appointments) vào luồng tạo lịch tái khám. Báo lỗi `InvalidOperationException` lập tức nếu phát hiện khung giờ không hợp lệ.

## [BUG-UI-001] Thiếu thông tin Tiền sử, Khám lâm sàng và Kế hoạch điều trị trong Lịch sử y tế
- **Trạng thái:** FIXED
- **Thời gian:** 25-07-2026
### Nguyên nhân
Tại màn hình Lịch sử y tế của Khách hàng (`MyHistoryTab.vue`), ứng dụng chỉ hiển thị "Chẩn đoán của bác sĩ" (A) và "Lời dặn dò" (P) thay vì hiển thị toàn bộ 4 mục SOAP. Phía Backend API `CustomerAppointmentController` chỉ trả về mỗi `Diagnosis`.
### Giải pháp
1. Cập nhật `MedicalRecordCustomerViewDto` bổ sung các trường `MedicalHistory`, `ClinicalSigns`, và `TreatmentPlan`.
2. Map dữ liệu trong `CustomerAppointmentController.cs` bằng `ExtractReadableSoap` cho các trường `S`, `O`, và `P`.
3. Bổ sung giao diện trong `MyHistoryTab.vue` để hiển thị thành phần `Comprehensive Medical Details (SOAP)` đầy đủ thay vì `Simplified Medical Details`.

## [BUG-SCHEDULE-001] Có thể xếp ca trực vào ngày bác sĩ đã xin nghỉ phép
- **Trạng thái:** FIXED
- **Thời gian:** 26-07-2026
### Nguyên nhân
Trong `DoctorScheduleService`, các hàm tạo và cập nhật ca trực (`CreateScheduleAsync`, `UpdateScheduleAsync`) mới chỉ kiểm tra trùng lặp thời gian giữa các ca trực (Overlap schedules) với nhau, mà không đối chiếu với các đơn xin nghỉ phép đã được duyệt của bác sĩ trong bảng `ScheduleExceptions`.
### Giải pháp
Bổ sung logic truy vấn `_unitOfWork.ScheduleExceptions.Query()` với điều kiện `Type == "TimeOff"` và `Status == "Approved"`. Chuyển đổi khung giờ trực thành `shiftStart` và `shiftEnd` tuyệt đối, sau đó kiểm tra thuật toán giao cắt thời gian `StartDate < shiftEnd && EndDate > shiftStart`. Nếu bị trùng, ném ra ngoại lệ `InvalidOperationException` để chặn việc xếp ca trực.

## [BUG-INVOICE-001] Lỗi hiển thị 0 số lượng tồn kho dù đã thêm lô thuốc
- **Trạng thái:** FIXED
- **Thời gian:** 01-08-2026
### Nguyên nhân
Khi nhập thêm lô thuốc mới thông qua `MedicineService.ImportMedicineAsync` và `AdjustMedicineStockAsync`, trường denormalized `StockQuantity` trên bảng `Medicine` đã không được cộng dồn (update) khiến nó bị kẹt ở giá trị cũ (hoặc 0). Đồng thời, API `GetCatalogItemsAsync` không dùng số lượng tự tính từ Batch để phòng ngừa lỗi sai lệch data.
### Giải pháp
1. Thêm đoạn code cập nhật `medicine.StockQuantity += quantity` vào cả 2 hàm import và adjust của `MedicineService`.
2. Sửa lại `InvoiceService.GetCatalogItemsAsync` để sử dụng `.Include(m => m.Batches)` và tính toán linh động số lượng tồn từ `Sum(CurrentQuantity)` của các lô còn hạn sử dụng.
3. Sửa lại logic trong hàm `AddInvoiceItemAsync` và `UpdateInvoiceItemQtyAsync` của `InvoiceService.cs` để query mảng `Batches` và tính toán lượng tồn kho thực tế, thay vì dựa vào trường `StockQuantity` lỗi thời của `Medicine`, đảm bảo data cũ bị kẹt StockQuantity 0 vẫn có thể thanh toán, thêm bớt hóa đơn bình thường.

## [BUG-UI-002] Lịch hẹn Pending (Chờ duyệt) hiển thị sai ở Lịch Trình Chi Tiết
- **Trạng thái:** FIXED
- **Thời gian:** 01-08-2026
### Nguyên nhân
Tại trang "Lịch Trình Chi Tiết" (Bảng lịch hẹn và điều phối), danh sách đáng lẽ chỉ hiển thị những lịch hẹn đã xác nhận hoặc đang tiến hành, nhưng hệ thống vẫn truy vấn và trả về cả những lịch hẹn ở trạng thái `pending` (Chờ duyệt).
### Giải pháp
Cập nhật API lấy sự kiện lịch (`GetCalendarEventsAsync`) tại cả 2 file `ReceptionistAppointmentService.cs` và `DoctorAppointmentService.cs` để loại bỏ các lịch hẹn có `Status == "pending"`. Các lịch hẹn chờ duyệt nay chỉ hiển thị trong tab "Yêu Cầu Chờ Duyệt" của lễ tân.

## [BUG-UI-003] Cập nhật số lượng sản phẩm trên hóa đơn bị chậm và giật lag (Terribly Slow)
- **Trạng thái:** FIXED
- **Thời gian:** 01-08-2026
### Nguyên nhân
Khi người dùng bấm nút `+` hoặc `-` nhiều lần liên tiếp để tăng/giảm số lượng sản phẩm (trong Component `InvoicesTab.vue`), hàm `changeQty` lập tức gọi API `PUT /invoice/items/{id}` ở mỗi lần click mà không có cơ chế chờ (debounce). Điều này dẫn đến việc Frontend gửi dồn dập hàng loạt request xuống Backend cùng lúc, gây nghẽn cổ chai DB do Backend phải liên tục Update và tính toán lại `Subtotal`, khiến giao diện bị "đơ" chờ phản hồi.
### Giải pháp
Sử dụng kỹ thuật **Optimistic UI Update kết hợp với Debounce** trong `changeQty`:
1. Ngay lập tức cập nhật `item.quantity` và `totalPrice` ở bộ nhớ tạm (Local Vue state) trên giao diện để người dùng thấy số nhảy lên mượt mà ngay tắp lự mà không bị delay.
2. Dùng `setTimeout(..., 400)` để dồn tất cả các thao tác bấm chuột liên tiếp lại. Backend API chỉ thực sự được gọi **sau khi người dùng ngừng bấm** được 0.4 giây. Nếu API báo lỗi (quá số lượng tồn kho), giao diện sẽ tự động gọi lại hàm `selectAppointment` để cuộn ngược (rollback) về dữ liệu chuẩn của Server.

## [BUG-INVOICE-002] Lỗi trùng mã lô thuốc (Batch Number) không phân biệt loại thuốc
- **Trạng thái:** FIXED
- **Thời gian:** 02-08-2026
### Nguyên nhân
Khi nhập thêm lô thuốc mới, API kiểm tra tính duy nhất của mã lô bằng hàm `GetBatchByNumberAsync` chỉ dựa vào `batchNumber` trên toàn cục kho. Do đó, nếu thuốc A có mã lô "L01", thuốc B cũng muốn có lô "L01" thì sẽ bị chặn lại vì thông báo "Số lô đã được sử dụng cho một loại thuốc khác".
### Giải pháp
1. Cập nhật hàm `GetBatchByNumberAsync` thành `GetBatchByNumberAndMedicineAsync(string batchNumber, long medicineId)`.
2. Kiểm tra tính duy nhất của mã lô kết hợp đồng thời với `medicineId` trong Repository và Service, đảm bảo các loại thuốc khác nhau có thể sử dụng chung một mã lô độc lập.

## [BUG-OFFER-001] Lỗi không thể để trống Tổng số lượt dùng (Vô hạn) khi tạo Voucher
- **Trạng thái:** FIXED
- **Thời gian:** 25-08-2026
### Nguyên nhân
Khi người dùng xóa nội dung trong ô input Tổng số lượt dùng hoặc Giảm tối đa, Vue -model.number chuyển giá trị thành chuỗi rỗng "". Tại hàm saveOffer, đoạn code sanitize kiểm tra payload.totalQuantity === "" gây ra lỗi biên dịch TypeScript (vì kiểu dữ liệu là 
umber | null), làm cho logic gán 
ull (vô hạn) bị thất bại, hoặc không hoạt động chính xác nếu nhập 0.
### Giải pháp
Sửa đổi logic sanitize payload thành if (!payload.totalQuantity || payload.totalQuantity <= 0) để bỏ qua lỗi ép kiểu TypeScript, xử lý đồng thời chuỗi rỗng "" lẫn các giá trị không hợp lệ (<= 0). Các giá trị này đều được an toàn gán thành 
ull (không giới hạn) trước khi gửi về Backend.
