using System;

namespace MyPetClinic.Application.Utils
{
    public static class EmailTemplateBuilder
    {
        private static string WrapInBaseTemplate(string title, string content)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        .info-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; border: 1px solid #eeeeee; border-radius: 8px; overflow: hidden; }}
        .info-table th, .info-table td {{ padding: 12px 15px; border-bottom: 1px solid #eeeeee; text-align: left; font-size: 14px; }}
        .info-table tr:last-child th, .info-table tr:last-child td {{ border-bottom: none; }}
        .info-table th {{ width: 40%; background-color: #fef9e7; color: #d4ac0d; font-weight: bold; }}
        .info-table td {{ background-color: #ffffff; color: #555; }}
        p {{ color: #555; line-height: 1.6; font-size: 15px; margin-bottom: 15px; margin-top: 0; }}
        b, strong {{ color: #2c3e50; font-weight: bold; }}
    </style>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f6f9; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 10px; border-top: 5px solid #f1c40f; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 20px;'>
            <h2 style='color: #2c3e50; margin: 0; font-size: 28px;'>MyPet<span style='color: #f1c40f;'>Clinic</span></h2>
        </div>
        <h3 style='color: #2c3e50; font-size: 18px; margin-bottom: 15px;'>{title}</h3>
        <div style='color: #555; line-height: 1.6; font-size: 15px;'>
            {content}
        </div>
        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0 20px 0;'>
        <p style='color: #95a5a6; font-size: 13px; text-align: center; margin: 0;'>Email này được gửi tự động từ hệ thống MyPetClinic.<br>Vui lòng không trả lời thư này.</p>
    </div>
</body>
</html>";
        }

        public static string BuildAppointmentConfirmedEmail(string customerName, string petName, DateTime appointmentDate, string doctorName, string timeSlot, string qrToken = "")
        {
            var title = "Xác Nhận Lịch Hẹn Thành Công";
            var qrHtml = string.IsNullOrEmpty(qrToken) ? "" : $"<tr><th>Mã đặt lịch</th><td><b style='color: #0d6efd; font-size: 16px;'>{qrToken}</b> <small>(Vui lòng đưa mã này cho Lễ tân khi đến)</small></td></tr>";
            
            var content = $@"
                <p>Chào <b>{customerName}</b>,</p>
                <p>Phòng khám MyPetClinic đã nhận và xác nhận lịch hẹn của bạn cho thú cưng <b>{petName}</b>. Dưới đây là thông tin chi tiết:</p>
                <table class='info-table'>
                    <tr><th>Khách hàng</th><td>{customerName}</td></tr>
                    <tr><th>Thú cưng</th><td>{petName}</td></tr>
                    <tr><th>Bác sĩ phụ trách</th><td>{doctorName}</td></tr>
                    <tr><th>Ngày khám</th><td>{appointmentDate:dd/MM/yyyy}</td></tr>
                    <tr><th>Giờ khám</th><td>{timeSlot}</td></tr>
                    {qrHtml}
                </table>
                <p>Vui lòng đến đúng giờ để phòng khám phục vụ bạn tốt nhất. Nếu có thay đổi, vui lòng đăng nhập vào hệ thống để thao tác hoặc liên hệ hotline của chúng tôi.</p>
                <p>Trân trọng cảm ơn!</p>
            ";
            return WrapInBaseTemplate(title, content);
        }

        public static string BuildAppointmentCancelledEmail(string customerName, string petName, DateTime appointmentDate, string reason)
        {
            var title = "Thông Báo Hủy Lịch Hẹn";
            var content = $@"
                <p>Chào <b>{customerName}</b>,</p>
                <p>Lịch hẹn của thú cưng <b>{petName}</b> vào ngày <b>{appointmentDate:dd/MM/yyyy}</b> đã bị hủy.</p>
                <p><b>Lý do hủy:</b> {reason}</p>
                <p>Chúng tôi rất tiếc vì sự cố này. Bạn có thể đặt một lịch hẹn mới trực tiếp trên hệ thống website của chúng tôi.</p>
                <p>Cảm ơn bạn đã thông cảm.</p>
            ";
            return WrapInBaseTemplate(title, content);
        }

        public static string BuildAppointmentRescheduledEmail(string customerName, string petName, DateTime oldDate, DateTime newDate, string newTimeSlot)
        {
            var title = "Thông Báo Dời Lịch Hẹn";
            var content = $@"
                <p>Chào <b>{customerName}</b>,</p>
                <p>Lịch hẹn của thú cưng <b>{petName}</b> đã được thay đổi.</p>
                <table class='info-table'>
                    <tr><th>Lịch cũ</th><td><del>{oldDate:dd/MM/yyyy}</del></td></tr>
                    <tr><th>Lịch mới</th><td><b>{newDate:dd/MM/yyyy}</b></td></tr>
                    <tr><th>Giờ khám mới</th><td><b>{newTimeSlot}</b></td></tr>
                </table>
                <p>Mong bạn lưu ý thời gian mới để đưa bé đến phòng khám đúng giờ nhé.</p>
                <p>Trân trọng!</p>
            ";
            return WrapInBaseTemplate(title, content);
        }

        public static string BuildOtpEmail(string title, string fullName, string otp, string messageBody)
        {
            var content = $@"
                <p>Xin chào <b>{fullName}</b>,</p>
                <p>{messageBody}</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <div style='display: inline-block; padding: 15px 40px; background-color: #fef9e7; border: 2px dashed #f1c40f; border-radius: 8px; font-size: 32px; font-weight: bold; color: #d4ac0d; letter-spacing: 8px;'>
                        {otp}
                    </div>
                </div>
                <p>Mã OTP này sẽ hết hạn trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai để đảm bảo an toàn.</p>
            ";
            return WrapInBaseTemplate(title, content);
        }

        public static string BuildThankYouInvoiceEmail(
            string customerName, 
            string invoiceCode, 
            decimal totalAmount,
            string? petName,
            string? doctorName,
            DateTime? appointmentDate,
            IEnumerable<(string ItemName, int Quantity, decimal TotalPrice)> items,
            decimal discountAmount = 0)
        {
            var title = "Cảm Ơn Bạn Đã Sử Dụng Dịch Vụ";
            
            var itemsHtml = "";
            foreach(var item in items)
            {
                itemsHtml += $@"
                    <tr>
                        <td style='padding: 12px 15px; border-bottom: 1px dashed #eee;'>{item.ItemName}</td>
                        <td style='padding: 12px 15px; border-bottom: 1px dashed #eee; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 12px 15px; border-bottom: 1px dashed #eee; text-align: right;'>{item.TotalPrice:N0} đ</td>
                    </tr>";
            }

            string dateStr = appointmentDate.HasValue ? appointmentDate.Value.ToString("dd/MM/yyyy") : "N/A";
            string doctorStr = string.IsNullOrEmpty(doctorName) ? "Chưa chỉ định" : doctorName;
            string petStr = string.IsNullOrEmpty(petName) ? "Không có" : petName;

            string discountHtml = "";
            if (discountAmount > 0)
            {
                discountHtml = $@"
                        <tr>
                            <td colspan='2' style='padding: 10px 15px; text-align: right; color: #2c3e50; font-size: 14px;'>Khuyến mãi / Voucher:</td>
                            <td style='padding: 10px 15px; text-align: right; color: #27ae60; font-weight: bold;'>-{discountAmount:N0} đ</td>
                        </tr>";
            }

            var content = $@"
                <p>Chào <b>{customerName}</b>,</p>
                <p>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ tại MyPetClinic. Quá trình thăm khám đã hoàn tất và hóa đơn của bạn đã được thanh toán thành công.</p>
                
                <table class='info-table'>
                    <tr><th>Mã hóa đơn</th><td>{invoiceCode}</td></tr>
                    <tr><th>Ngày khám</th><td>{dateStr}</td></tr>
                    <tr><th>Thú cưng</th><td>{petStr}</td></tr>
                    <tr><th>Bác sĩ phụ trách</th><td>{doctorStr}</td></tr>
                </table>

                <h4 style='color: #2c3e50; margin-top: 30px; margin-bottom: 10px; border-bottom: 2px solid #f1c40f; padding-bottom: 5px; display: inline-block;'>Chi tiết dịch vụ & thuốc</h4>
                <table style='width: 100%; border-collapse: collapse; margin-bottom: 25px; font-size: 14px;'>
                    <thead>
                        <tr style='background-color: #f4f6f9;'>
                            <th style='padding: 12px 15px; border-bottom: 2px solid #ddd; text-align: left; color: #2c3e50;'>Mô tả</th>
                            <th style='padding: 12px 15px; border-bottom: 2px solid #ddd; text-align: center; color: #2c3e50;'>SL</th>
                            <th style='padding: 12px 15px; border-bottom: 2px solid #ddd; text-align: right; color: #2c3e50;'>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                    <tfoot>
                        {discountHtml}
                        <tr>
                            <td colspan='2' style='padding: 15px 15px; text-align: right; font-weight: bold; color: #2c3e50;'>Tổng thanh toán:</td>
                            <td style='padding: 15px 15px; text-align: right; font-weight: bold; color: #d35400; font-size: 16px;'>{totalAmount:N0} đ</td>
                        </tr>
                    </tfoot>
                </table>

                <p>Bạn có thể theo dõi chi tiết đơn thuốc và hồ sơ bệnh án của bé trong mục quản lý tài khoản trên website.</p>
                <p>Chúc bé cưng của bạn luôn khỏe mạnh và năng động!</p>
            ";
            return WrapInBaseTemplate(title, content);
        }
    }
}
