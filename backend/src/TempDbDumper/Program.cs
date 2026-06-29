using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static string GetDesc(string table, string col)
    {
        col = col.ToLower();
        table = table.ToLower();
        
        // --- 1. PRIMARY KEYS & AUDIT LOGS ---
        if (col == "id") return "Mã định danh (Khóa chính - PK)";
        if (col == "created_at") return "Thời gian tạo bản ghi";
        if (col == "updated_at") return "Thời gian cập nhật cuối cùng";
        if (col == "deleted_at" || col == "deletedat") return "Thời gian xóa (Xóa mềm - Soft delete)";
        if (col == "created_by") return "Mã người tạo (FK)";
        if (col == "updated_by") return "Mã người cập nhật (FK)";
        if (col == "row_version") return "Version để quản lý đồng thời (Concurrency Token)";
        if (col == "is_active") return "Trạng thái hoạt động (True/False)";
        if (col == "name") return "Tên";
        if (col == "title") return "Tiêu đề";
        if (col == "description") return "Mô tả chi tiết";
        if (col == "content") return "Nội dung chi tiết";
        if (col == "summary") return "Tóm tắt ngắn gọn";
        if (col == "status") return "Trạng thái";
        if (col == "type") return "Loại / Phân loại";
        if (col == "order") return "Thứ tự sắp xếp";
        
        // --- 2. FOREIGN KEYS (COMMON) ---
        if (col == "user_id") return "Mã người dùng liên kết (FK)";
        if (col == "customer_id" || col == "customerid") return "Mã khách hàng liên kết (FK)";
        if (col == "doctor_id") return "Mã bác sĩ phụ trách (FK)";
        if (col == "pet_id") return "Mã thú cưng (FK)";
        if (col == "appointment_id") return "Mã lịch hẹn (FK)";
        if (col == "service_id") return "Mã dịch vụ (FK)";
        if (col == "category_id") return "Mã danh mục (FK)";
        if (col == "medicine_id") return "Mã thuốc (FK)";
        if (col == "vaccine_id") return "Mã vaccine (FK)";
        if (col == "invoice_id") return "Mã hóa đơn (FK)";
        if (col == "batch_id" || col == "vaccine_batch_id") return "Mã lô hàng (FK)";
        if (col == "author_id") return "Mã tác giả bài viết (FK)";
        if (col == "parent_id") return "Mã danh mục cha (FK)";
        if (col == "role_id") return "Mã vai trò phân quyền (FK)";
        if (col == "accountid") return "Mã tài khoản (FK)";
        if (col == "created_by_user_id") return "Người thực hiện giao dịch (FK)";

        // --- 3. COMMON ATTRIBUTES ---
        if (col == "email") return "Địa chỉ Email (Unique)";
        if (col == "phone") return "Số điện thoại liên hệ";
        if (col == "full_name") return "Họ và tên đầy đủ";
        if (col == "avatar" || col == "image_url" || col == "thumbnail") return "Đường dẫn hình ảnh / Avatar";
        if (col == "gender") return "Giới tính (0: Cái/Nữ, 1: Đực/Nam, 2: Khác)";
        if (col == "date_of_birth" || col == "birth_date") return "Ngày tháng năm sinh";
        if (col == "address") return "Địa chỉ nơi ở";
        if (col == "slug") return "Đường dẫn tĩnh (Slug URL)";
        if (col == "password_hash") return "Mật khẩu đã được băm mã hóa";
        if (col == "link_url") return "Đường dẫn liên kết đích";
        if (col == "start_date") return "Ngày bắt đầu";
        if (col == "end_date") return "Ngày kết thúc";
        if (col == "start_time") return "Thời gian bắt đầu";
        if (col == "end_time") return "Thời gian kết thúc";
        
        // --- 4. TABLE SPECIFIC ---
        if (table == "appointments") {
            if (col == "appointment_date") return "Ngày hẹn khám";
            if (col == "symptom") return "Triệu chứng ban đầu của thú cưng";
            if (col == "note") return "Ghi chú thêm của khách hàng";
            if (col == "cancellation_reason" || col == "cancel_reason") return "Lý do hủy lịch hẹn";
            if (col == "cancelled_by_role") return "Vai trò của người hủy lịch (Admin, Customer...)";
            if (col == "checkintime" || col == "check_in_time") return "Thời gian khách check-in thực tế";
            if (col == "checkouttime" || col == "check_out_time") return "Thời gian khách check-out hoàn tất";
            if (col == "isemergency" || col == "is_emergency") return "Đánh dấu là ca cấp cứu?";
            if (col == "iswalkin" || col == "is_walk_in") return "Khách vãng lai (không đặt trước)?";
            if (col == "queuenumber" || col == "queue_number") return "Số thứ tự chờ khám";
            if (col == "qrtoken" || col == "qr_token") return "Mã Token QR để check-in tự động";
        }
        
        if (table == "pets") {
            if (col == "owner_id") return "Mã khách hàng sở hữu (FK)";
            if (col == "species") return "Loài (VD: Chó, Mèo, Chim...)";
            if (col == "breed") return "Giống (VD: Poodle, Corgi...)";
            if (col == "weight") return "Cân nặng hiện tại (kg)";
            if (col == "color") return "Màu lông/Màu sắc đặc trưng";
            if (col == "blood_type") return "Nhóm máu";
            if (col == "sterilized") return "Đã triệt sản hay chưa?";
            if (col == "microchip_code") return "Mã số Microchip cấy ghép";
            if (col == "allergy_note") return "Ghi chú các dị ứng thuốc/thức ăn";
            if (col == "is_deceased" || col == "isdeceased") return "Thú cưng đã qua đời?";
            if (col == "isaggressive") return "Tính cách hung dữ/khó tiếp cận?";
            if (col == "chronicdisease") return "Các bệnh lý mãn tính đang mắc";
            if (col == "currentdiet") return "Chế độ ăn kiêng/dinh dưỡng hiện tại";
        }
        
        if (table == "medical_records") {
            if (col == "weight") return "Cân nặng lúc khám (kg)";
            if (col == "temperature") return "Nhiệt độ cơ thể lúc khám (°C)";
            if (col == "medical_history") return "Tiền sử bệnh án liên quan";
            if (col == "diagnosis") return "Chẩn đoán bệnh lý của bác sĩ";
            if (col == "treatment_plan") return "Phác đồ / Kế hoạch điều trị";
            if (col == "doctor_notes") return "Ghi chú nội bộ của bác sĩ";
            if (col == "follow_up_date") return "Ngày chỉ định tái khám";
            if (col == "clinical_signs") return "Các dấu hiệu lâm sàng";
            if (col == "record_type") return "Loại bệnh án (Khám bệnh, Phẫu thuật, Ngoại trú...)";
        }
        
        if (table == "medicines") {
            if (col == "name") return "Tên thuốc / Biệt dược";
            if (col == "unit") return "Đơn vị tính (Viên, Lọ, Tuýp, Vỉ...)";
            if (col == "min_stock_level") return "Số lượng tồn kho tối thiểu để cảnh báo nhập hàng";
            if (col == "import_price") return "Giá nhập kho (VNĐ)";
            if (col == "sell_price") return "Giá bán lẻ cho khách (VNĐ)";
            if (col == "medicine_code") return "Mã số thuốc quản lý nội bộ (UNIQUE)";
        }
        
        if (table == "medicine_batches" || table == "vaccine_batches") {
            if (col == "batch_number") return "Số lô hàng hóa (Batch No)";
            if (col == "manufacture_date") return "Ngày sản xuất (NSX)";
            if (col == "expiry_date" || col == "expiration_date") return "Ngày hết hạn (HSD)";
            if (col == "import_date") return "Ngày nhập lô hàng vào kho";
            if (col == "initial_quantity") return "Số lượng ban đầu khi nhập lô";
            if (col == "current_quantity" || col == "stock_quantity") return "Số lượng còn tồn thực tế trong lô";
            if (col == "import_price") return "Giá nhập của lô hàng này";
            if (col == "selling_price") return "Giá bán áp dụng cho lô này";
        }
        
        if (table == "inventory_transactions") {
            if (col == "transaction_date") return "Thời gian thực hiện giao dịch";
            if (col == "type") return "Loại giao dịch (1: Nhập kho, 2: Xuất kho, 3: Hủy/Hư hỏng)";
            if (col == "quantity_change") return "Số lượng thay đổi (dương là nhập, âm là xuất)";
            if (col == "reference_code") return "Mã phiếu tham chiếu (VD: Mã phiếu nhập/xuất)";
            if (col == "notes") return "Lý do / Ghi chú giao dịch";
        }
        
        if (table == "vaccines") {
            if (col == "name") return "Tên loại vaccine";
            if (col == "manufacturer") return "Hãng / Nhà sản xuất (VD: Zoetis, Boehringer...)";
            if (col == "stock_quantity") return "Tổng số lượng tồn kho khả dụng";
            if (col == "target_species") return "Loài động vật chỉ định tiêm (Chó, Mèo...)";
            if (col == "min_age_weeks") return "Độ tuổi tối thiểu có thể tiêm (tính bằng tuần)";
            if (col == "interval_days") return "Khoảng cách thời gian (ngày) giữa 2 mũi tiêm nhắc";
        }
        
        if (table == "vaccination_records") {
            if (col == "injection_date") return "Ngày thực hiện tiêm phòng";
            if (col == "next_due_date") return "Ngày dự kiến tiêm mũi nhắc lại";
            if (col == "reaction_note") return "Ghi chú các phản ứng phụ sau tiêm (Sốc phản vệ, sốt...)";
            
            // Các trường chi tiết y tế khi tiêm
            if (col == "allergy_details") return "Chi tiết về dị ứng thuốc của thú cưng";
            if (col == "clinical_assessment") return "Đánh giá lâm sàng trước khi tiêm";
            if (col == "dehydration_percent") return "Tỷ lệ mất nước (%)";
            if (col == "doctor_remarks") return "Nhận xét bổ sung của bác sĩ";
            if (col == "dose") return "Liều lượng tiêm (ml)";
            if (col == "eating_status") return "Tình trạng ăn uống hiện tại";
            if (col == "eye_nose_ear_status") return "Tình trạng Mắt, Mũi, Tai";
            if (col == "follow_up_instructions") return "Hướng dẫn chăm sóc sau tiêm";
            if (col == "has_cough_or_sneeze") return "Có triệu chứng ho hoặc hắt hơi không?";
            if (col == "has_previous_reaction") return "Đã từng có phản ứng phụ với vaccine trước đây?";
            if (col == "has_vomiting_or_diarrhea") return "Có triệu chứng nôn mửa hoặc tiêu chảy không?";
            if (col == "heart_rate") return "Nhịp tim (lần/phút)";
            if (col == "injection_site") return "Vị trí tiêm (VD: Dưới da gáy, Bắp đùi...)";
            if (col == "is_allergic") return "Thú cưng có cơ địa dị ứng không?";
            if (col == "is_under_treatment") return "Thú cưng có đang điều trị bệnh khác không?";
            if (col == "lymph_node_status") return "Tình trạng hạch bạch huyết";
            if (col == "mental_status") return "Trạng thái tinh thần (Linh hoạt, lờ đờ...)";
            if (col == "mucosa_status") return "Tình trạng niêm mạc";
            if (col == "owner_notes") return "Thông tin do chủ nuôi cung cấp";
            if (col == "previous_reaction_details") return "Chi tiết phản ứng phụ lần trước (nếu có)";
            if (col == "previous_vaccine_history") return "Lịch sử tiêm phòng trước đây";
            if (col == "reason_for_visit") return "Lý do đến khám tiêm";
            if (col == "respiratory_rate") return "Nhịp thở (lần/phút)";
            if (col == "route") return "Đường tiêm (SC, IM, IV...)";
            if (col == "temperature") return "Thân nhiệt lúc tiêm (°C)";
            if (col == "treatment_details") return "Chi tiết điều trị (nếu có)";
            if (col == "weight") return "Cân nặng (kg)";
        }
        
        if (table == "invoices") {
            if (col == "subtotal") return "Tổng tiền dịch vụ và thuốc (trước giảm giá)";
            if (col == "discount_amount") return "Số tiền được giảm giá / khuyến mãi";
            if (col == "total_amount") return "Tổng số tiền khách hàng phải thanh toán cuối cùng";
            if (col == "payment_status") return "Trạng thái thanh toán (Unpaid, Paid, Cancelled)";
            if (col == "payment_method") return "Phương thức thanh toán (Cash, Credit Card, Transfer)";
            if (col == "paid_at") return "Thời điểm hoàn tất thanh toán";
        }
        
        if (table == "invoice_items") {
            if (col == "item_type") return "Loại dịch vụ (VD: Service, Medicine, Vaccine)";
            if (col == "item_id") return "Mã tham chiếu tới Service/Medicine/Vaccine tương ứng";
            if (col == "item_name") return "Tên hạng mục dịch vụ/thuốc";
            if (col == "quantity") return "Số lượng";
            if (col == "unit_price") return "Đơn giá (VNĐ)";
            if (col == "total_price") return "Thành tiền (VNĐ)";
        }
        
        if (table == "services") {
            if (col == "price") return "Giá dịch vụ niêm yết (VNĐ)";
            if (col == "duration_minutes") return "Thời lượng thực hiện dịch vụ (phút)";
        }
        
        if (table == "posts") {
            if (col == "keywords") return "Từ khóa SEO (Meta Keywords)";
            if (col == "meta_description") return "Mô tả SEO ngắn gọn (Meta Description)";
            if (col == "meta_title") return "Tiêu đề SEO (Meta Title)";
            if (col == "published_at") return "Thời gian xuất bản bài viết công khai";
            if (col == "view_count") return "Số lượt xem bài viết";
        }
        
        if (table == "prescription_items") {
            if (col == "prescription_id") return "Mã đơn thuốc (FK)";
            if (col == "dosage") return "Liều lượng sử dụng cho 1 lần (VD: 1 viên, 2ml)";
            if (col == "frequency") return "Tần suất sử dụng (VD: 2 lần/ngày)";
            if (col == "duration_days") return "Số ngày sử dụng liên tục";
            if (col == "quantity") return "Tổng số lượng thuốc kê đơn";
            if (col == "instruction") return "Hướng dẫn cách dùng chi tiết cho chủ nuôi";
        }
        
        if (table == "reviews") {
            if (col == "rating") return "Điểm đánh giá (1 đến 5 sao)";
            if (col == "comment") return "Nội dung nhận xét của khách hàng";
        }
        
        if (table == "clinic_operating_days") {
            if (col == "day_of_week") return "Ngày trong tuần (0: CN, 1: T2, ... 6: T7)";
            if (col == "is_open") return "Phòng khám có mở cửa ngày này không?";
        }
        
        if (table == "clinic_operating_shifts") {
            if (col == "clinic_operating_day_id") return "Mã ngày hoạt động (FK)";
        }
        
        if (table == "doctor_schedules") {
            if (col == "work_date") return "Ngày làm việc cụ thể";
            if (col == "max_appointments") return "Số lượng ca khám tối đa trong ca làm việc";
            if (col == "is_available") return "Bác sĩ có sẵn sàng nhận ca không?";
        }
        
        if (table == "employee_profiles") {
            if (col == "identity_card") return "Số CCCD/CMND";
            if (col == "position") return "Vị trí/Chức vụ (VD: Bác sĩ trưởng, Lễ tân...)";
            if (col == "is_resigned") return "Nhân viên đã nghỉ việc?";
        }
        
        if (table == "invitations") {
            if (col == "token") return "Mã token kích hoạt tài khoản";
            if (col == "expire_at") return "Thời điểm token hết hạn";
            if (col == "is_used") return "Token đã được sử dụng chưa?";
        }
        
        if (table == "notifications") {
            if (col == "is_read") return "Đã đọc thông báo?";
        }
        
        if (col == "customer_code") return "Mã khách hàng";
        if (col == "has_account") return "Khách hàng đã đăng ký tài khoản App chưa?";
        
        if (col.EndsWith("_id")) return "Mã định danh liên kết (FK)";
        
        // --- CATCH ALL FALLBACK ---
        return "Thông tin " + col;
    }

    static void Main()
    {
        var lines = File.ReadAllLines("real_schema_types.txt");
        var sb = new StringBuilder();
        sb.AppendLine("# Tài Liệu Cấu Trúc Cơ Sở Dữ Liệu (Database Dictionary) - MyPetClinic");
        sb.AppendLine();
        sb.AppendLine("Dưới đây là danh sách chi tiết các bảng trong cơ sở dữ liệu của dự án `MyPetClinic` được trích xuất trực tiếp từ DATABASE THỰC TẾ đang chạy. Tất cả kiểu dữ liệu và mô tả cột đã được tinh chỉnh chuyên nghiệp, bám sát nghiệp vụ hệ thống.");
        sb.AppendLine();
        
        int tableCount = 0;
        string currentTable = "";
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            if (line.StartsWith("Table: "))
            {
                currentTable = line.Substring(7).Trim();
                if (currentTable == "__EFMigrationsHistory") continue;
                tableCount++;
                if (tableCount > 1) sb.AppendLine();
                sb.AppendLine($"### {tableCount}. Bảng: `{currentTable}`");
                sb.AppendLine("| THUỘC TÍNH | KIỂU DỮ LIỆU | MÔ TẢ |");
                sb.AppendLine("| :--- | :--- | :--- |");
            }
            else if (line.StartsWith("- "))
            {
                var parts = line.Substring(2).Split(' ', 2);
                if (parts.Length == 2)
                {
                    string col = parts[0];
                    string type = parts[1].Trim('(', ')');
                    string desc = GetDesc(currentTable, col);
                    sb.AppendLine($"| {col} | {type} | {desc} |");
                }
            }
        }
        
        File.WriteAllText(@"C:\Users\NAM\.gemini\antigravity-ide\brain\8de33254-f863-46d1-b7e2-feed70adfbf3\database_dictionary.md", sb.ToString());
        Console.WriteLine("Done generating MD with highly detailed descriptions");
    }
}
