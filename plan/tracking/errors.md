
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

