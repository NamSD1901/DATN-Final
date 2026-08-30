using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailQueue _emailQueue;

        public InvoiceService(IUnitOfWork unitOfWork, IEmailQueue emailQueue)
        {
            _unitOfWork = unitOfWork;
            _emailQueue = emailQueue;
        }

        public async Task<IEnumerable<QueueItemDto>> GetPendingCheckoutsAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            // Get all appointments from today that are ready to pay or completed but invoice is unpaid
            var appointmentsList = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.AppointmentDate.Date == today &&
                            (a.Status == "ready_to_pay" || 
                             (a.Status == "completed" && (a.Invoice == null || a.Invoice.PaymentStatus != "paid"))),
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Invoice!
            );
                
            var appointments = appointmentsList
                .OrderByDescending(a => a.IsEmergency)
                .ThenBy(a => a.QueueNumber)
                .ToList();

            return appointments.Select(a => new QueueItemDto
            {
                AppointmentId = a.Id,
                PetId = a.PetId,
                PetName = a.Pet?.Name,
                Species = a.Pet?.Species,
                Weight = a.Pet?.Weight,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.FullName,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.FullName,
                Status = a.Status,
                Symptom = a.Symptom,
                IsEmergency = a.IsEmergency,
                IsWalkIn = a.IsWalkIn,
                QueueNumber = a.QueueNumber,
                CheckInTime = a.CheckInTime,
                IsAggressive = a.Pet?.IsAggressive ?? false
            }).ToList();
        }

        public async Task<InvoiceDto> GetOrCreateInvoiceAsync(long appointmentId)
        {
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.AppointmentId == appointmentId,
                i => i.InvoiceItems,
                i => i.Appointment!.Customer!,
                i => i.Appointment!.Pet!,
                i => i.Appointment!.Doctor!
            );

            if (invoice != null)
            {
                return MapToDto(invoice);
            }

            // Create new invoice with auto-charge capture
            var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                a => a.Id == appointmentId,
                a => a.Customer!, a => a.Pet!, a => a.Doctor!, a => a.Service!
            );

            if (appointment == null)
            {
                throw new InvalidOperationException("Không tìm thấy lịch hẹn.");
            }

            invoice = new Invoice
            {
                AppointmentId = appointmentId,
                PaymentStatus = "unpaid",
                CreatedAt = DateTime.UtcNow
            };

            // 1. Add Service Charge
            if (appointment.Service != null)
            {
                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    ItemType = "service",
                    ItemId = appointment.ServiceId,
                    ItemName = appointment.Service.Name,
                    Quantity = 1,
                    UnitPrice = appointment.Service.Price ?? 0,
                    TotalPrice = appointment.Service.Price ?? 0
                });
            }

            // 2. Add Prescribed Medicines (from SOAP Medical Record)
            var medicalRecord = await _unitOfWork.MedicalRecords.GetFirstOrDefaultWithIncludesAsync(
                mr => mr.AppointmentId == appointmentId
            );

            if (medicalRecord != null)
            {
                var prescriptions = await _unitOfWork.Prescriptions.FindWithIncludesAsync(
                    p => p.MedicalRecordId == medicalRecord.Id
                );

                var prescriptionIds = prescriptions.Select(p => p.Id).ToList();
                if (prescriptionIds.Any())
                {
                    var prescriptionItems = await _unitOfWork.PrescriptionItems.FindWithIncludesAsync(
                        pi => prescriptionIds.Contains(pi.PrescriptionId),
                        pi => pi.Medicine!
                    );

                    foreach (var pi in prescriptionItems)
                    {
                        if (pi.Medicine != null)
                        {
                            invoice.InvoiceItems.Add(new InvoiceItem
                            {
                                ItemType = "medicine",
                                ItemId = pi.MedicineId,
                                ItemName = pi.Medicine.Name,
                                Quantity = pi.Quantity ?? 1,
                                UnitPrice = pi.Medicine.SellPrice,
                                TotalPrice = (pi.Quantity ?? 1) * pi.Medicine.SellPrice
                            });
                        }
                    }
                }
            }

            // Calculate totals
            invoice.Subtotal = invoice.InvoiceItems.Sum(ii => ii.TotalPrice);
            invoice.TotalAmount = invoice.Subtotal;

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.Id == invoice.Id,
                i => i.InvoiceItems,
                i => i.Appointment!.Customer!,
                i => i.Appointment!.Pet!,
                i => i.Appointment!.Doctor!
            );

            return invoice != null ? MapToDto(invoice) : throw new InvalidOperationException("Lỗi truy xuất hóa đơn vừa tạo.");
        }

        public async Task<InvoiceDto> AddInvoiceItemAsync(long invoiceId, string itemType, long itemId, int quantity)
        {
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.Id == invoiceId,
                i => i.InvoiceItems
            );

            if (invoice == null)
            {
                throw new InvalidOperationException("Không tìm thấy hóa đơn.");
            }

            if (invoice.PaymentStatus == "paid")
            {
                throw new InvalidOperationException("Không thể chỉnh sửa hóa đơn đã thanh toán.");
            }

            string itemName = "";
            decimal unitPrice = 0;

            var existingItem = invoice.InvoiceItems
                .FirstOrDefault(ii => ii.ItemType == itemType && ii.ItemId == itemId);

            if (itemType == "service")
            {
                var service = await _unitOfWork.Services.GetByIdAsync(itemId);
                if (service == null) throw new InvalidOperationException("Không tìm thấy dịch vụ.");
                itemName = service.Name;
                unitPrice = service.Price ?? 0;
            }
            else if (itemType == "medicine")
            {
                var medicine = await _unitOfWork.Medicines.Query().Include(m => m.Batches).FirstOrDefaultAsync(m => m.Id == itemId);
                if (medicine == null) throw new InvalidOperationException("Không tìm thấy thuốc.");
                
                var actualStock = medicine.Batches != null && medicine.Batches.Any() ? medicine.Batches.Where(b => b.ExpiryDate > DateTime.UtcNow).Sum(b => b.CurrentQuantity) : medicine.StockQuantity;
                var currentQty = existingItem != null ? existingItem.Quantity : 0;
                if (currentQty + quantity > actualStock)
                {
                    throw new InvalidOperationException(actualStock == 0 
                        ? "Sản phẩm này đã hết hàng." 
                        : $"Chỉ có thể thêm tối đa {actualStock - currentQty} sản phẩm nữa (trong kho còn {actualStock}).");
                }
                
                itemName = medicine.Name;
                unitPrice = medicine.SellPrice;
            }
            else
            {
                throw new InvalidOperationException("Loại mặt hàng không hợp lệ.");
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    ItemType = itemType,
                    ItemId = itemId,
                    ItemName = itemName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = quantity * unitPrice
                });
            }

            invoice.Subtotal = invoice.InvoiceItems.Sum(ii => ii.TotalPrice);
            invoice.TotalAmount = invoice.Subtotal - invoice.DiscountAmount;

            await _unitOfWork.SaveChangesAsync();

            // Fetch with full details
            return await GetInvoiceWithDetailsAsync(invoiceId);
        }

        public async Task<InvoiceDto> RemoveInvoiceItemAsync(long itemId)
        {
            var item = await _unitOfWork.InvoiceItems.GetFirstOrDefaultWithIncludesAsync(
                ii => ii.Id == itemId,
                ii => ii.Invoice!
            );

            if (item == null)
            {
                throw new InvalidOperationException("Không tìm thấy dòng chi phí.");
            }

            var invoice = item.Invoice;
            if (invoice == null) throw new InvalidOperationException("Không tìm thấy hóa đơn gắn kèm.");
            if (invoice.PaymentStatus == "paid")
            {
                throw new InvalidOperationException("Không thể chỉnh sửa hóa đơn đã thanh toán.");
            }

            _unitOfWork.InvoiceItems.Remove(item);
            await _unitOfWork.SaveChangesAsync();

            var invoiceId = invoice.Id;
            var freshInvoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.Id == invoiceId,
                i => i.InvoiceItems
            );

            if (freshInvoice != null)
            {
                freshInvoice.Subtotal = freshInvoice.InvoiceItems.Sum(ii => ii.TotalPrice);
                freshInvoice.TotalAmount = freshInvoice.Subtotal - freshInvoice.DiscountAmount;
                await _unitOfWork.SaveChangesAsync();
            }

            return await GetInvoiceWithDetailsAsync(invoiceId);
        }

        public async Task<InvoiceDto> UpdateInvoiceItemQtyAsync(long itemId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveInvoiceItemAsync(itemId);
            }

            var item = await _unitOfWork.InvoiceItems.GetFirstOrDefaultWithIncludesAsync(
                ii => ii.Id == itemId,
                ii => ii.Invoice!
            );

            if (item == null)
            {
                throw new InvalidOperationException("Không tìm thấy dòng chi phí.");
            }

            var invoice = item.Invoice;
            if (invoice == null) throw new InvalidOperationException("Không tìm thấy hóa đơn gắn kèm.");
            if (invoice.PaymentStatus == "paid")
            {
                throw new InvalidOperationException("Không thể chỉnh sửa hóa đơn đã thanh toán.");
            }

            if (item.ItemType == "medicine" && item.ItemId.HasValue)
            {
                var medicine = await _unitOfWork.Medicines.Query().Include(m => m.Batches).FirstOrDefaultAsync(m => m.Id == item.ItemId.Value);
                if (medicine != null)
                {
                    var actualStock = medicine.Batches != null && medicine.Batches.Any() ? medicine.Batches.Where(b => b.ExpiryDate > DateTime.UtcNow).Sum(b => b.CurrentQuantity) : medicine.StockQuantity;
                    if (quantity > actualStock)
                    {
                        throw new InvalidOperationException($"Chỉ còn {actualStock} sản phẩm trong kho.");
                    }
                }
            }

            item.Quantity = quantity;
            item.TotalPrice = quantity * item.UnitPrice;

            await _unitOfWork.SaveChangesAsync();

            // Refresh invoice total
            var invoiceId = invoice.Id;
            var freshInvoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.Id == invoiceId,
                i => i.InvoiceItems
            );

            if (freshInvoice != null)
            {
                freshInvoice.Subtotal = freshInvoice.InvoiceItems.Sum(ii => ii.TotalPrice);
                freshInvoice.TotalAmount = freshInvoice.Subtotal - freshInvoice.DiscountAmount;
                await _unitOfWork.SaveChangesAsync();
            }

            return await GetInvoiceWithDetailsAsync(invoiceId);
        }

        public async Task<bool> ProcessPaymentAsync(long invoiceId, string paymentMethod, decimal discountAmount, string? voucherCode = null)
        {
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.Id == invoiceId,
                i => i.Appointment!,
                i => i.Appointment!.Pet!,
                i => i.Appointment!.Doctor!,
                i => i.InvoiceItems
            );

            if (invoice == null) return false;
            if (invoice.PaymentStatus == "paid") return true;

            invoice.PaymentStatus = "paid";
            invoice.PaymentMethod = paymentMethod;
            invoice.DiscountAmount = discountAmount;
            invoice.TotalAmount = invoice.Subtotal - discountAmount;
            invoice.PaidAt = DateTime.UtcNow;

            if (invoice.Appointment != null)
            {
                invoice.Appointment.Status = "completed";
                invoice.Appointment.CheckOutTime = DateTime.UtcNow;
            }

            // Deduct medicine stock quantity if items are medicines
            var invoiceItems = await _unitOfWork.InvoiceItems.FindAsync(
                ii => ii.InvoiceId == invoiceId && ii.ItemType == "medicine"
            );

            foreach (var item in invoiceItems)
            {
                if (item.ItemId.HasValue)
                {
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.ItemId.Value);
                    if (medicine != null)
                    {
                        medicine.StockQuantity -= item.Quantity;
                        if (medicine.StockQuantity < 0) medicine.StockQuantity = 0; // prevent negative stock
                    }
                }
            }

            // Consume Voucher if provided
            if (!string.IsNullOrEmpty(voucherCode))
            {
                var offer = await _unitOfWork.Offers.Query().FirstOrDefaultAsync(o => o.Code == voucherCode);
                if (offer != null)
                {
                    offer.UsedQuantity++;
                    
                    if (invoice.Appointment != null)
                    {
                        var voucherString = $"[Áp dụng voucher: {offer.Code}]";
                        if (string.IsNullOrEmpty(invoice.Appointment.Note))
                        {
                            invoice.Appointment.Note = voucherString;
                        }
                        else if (!invoice.Appointment.Note.Contains(voucherString, StringComparison.OrdinalIgnoreCase))
                        {
                            invoice.Appointment.Note += "\n" + voucherString;
                        }

                        var customerUser = await _unitOfWork.Users.Query().FirstOrDefaultAsync(u => u.CustomerId == invoice.Appointment.CustomerId);
                        if (customerUser != null)
                        {
                            await _unitOfWork.OfferUsageLogs.AddAsync(new MyPetClinic.Domain.Entities.OfferUsageLog
                            {
                                OfferId = offer.Id,
                                UserId = customerUser.Id,
                                AppliedAt = DateTime.UtcNow,
                                Status = "APPLIED",
                                InvoiceId = invoice.Id,
                                DiscountApplied = discountAmount
                            });
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // TH4: Gửi email hóa đơn cảm ơn khi đã thanh toán thành công
            try
            {
                var customerId = invoice.Appointment?.CustomerId;
                if (customerId.HasValue)
                {
                    var customerUser = _unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == customerId.Value && u.IsActive == true);
                    if (customerUser != null && !string.IsNullOrEmpty(customerUser.Email))
                    {
                        string invoiceCode = "INV-" + invoice.Id.ToString("D5");
                        var itemsDto = invoice.InvoiceItems?.Select(ii => (ii.ItemName ?? "", ii.Quantity, ii.TotalPrice)) 
                                       ?? new List<(string, int, decimal)>();

                        var emailHtml = MyPetClinic.Application.Utils.EmailTemplateBuilder.BuildThankYouInvoiceEmail(
                            customerName: customerUser.FullName ?? invoice.Appointment?.Customer?.FullName ?? "Khách hàng",
                            invoiceCode: invoiceCode,
                            totalAmount: invoice.TotalAmount,
                            petName: invoice.Appointment?.Pet?.Name,
                            doctorName: invoice.Appointment?.Doctor?.FullName,
                            appointmentDate: invoice.Appointment?.AppointmentDate,
                            items: itemsDto,
                            discountAmount: invoice.DiscountAmount
                        );
                        await _emailQueue.QueueEmailAsync(new MyPetClinic.Application.DTOs.Notification.EmailMessageDto
                        {
                            ToEmail = customerUser.Email,
                            Subject = $"MyPetClinic - Cảm ơn bạn đã sử dụng dịch vụ ({invoiceCode})",
                            BodyHtml = emailHtml
                        });
                    }
                }
            }
            catch { /* Ignore error to not rollback */ }

            return true;
        }

        public async Task<IEnumerable<InvoiceCatalogItemDto>> GetCatalogItemsAsync(string query)
        {
            var list = new List<InvoiceCatalogItemDto>();
            var lowerQuery = (query ?? "").ToLower();

            var servicesQuery = _unitOfWork.Services.Query().Where(s => s.IsActive);
            if (!string.IsNullOrEmpty(lowerQuery))
            {
                servicesQuery = servicesQuery.Where(s => s.Name.ToLower().Contains(lowerQuery));
            }
            var services = servicesQuery.OrderBy(s => s.Name).Take(10).ToList();

            list.AddRange(services.Select(s => new InvoiceCatalogItemDto
            {
                Type = "service",
                Id = s.Id,
                Name = s.Name,
                Price = s.Price ?? 0,
                StockQuantity = 999
            }));

            IQueryable<Medicine> medicinesQuery = _unitOfWork.Medicines.Query().Include(m => m.Batches);
            if (!string.IsNullOrEmpty(lowerQuery))
            {
                medicinesQuery = medicinesQuery.Where(m => m.Name.ToLower().Contains(lowerQuery));
            }
            var medicines = medicinesQuery.OrderBy(m => m.Name).Take(10).ToList();

            list.AddRange(medicines.Select(m => new InvoiceCatalogItemDto
            {
                Type = "medicine",
                Id = m.Id,
                Name = m.Name,
                Price = m.SellPrice,
                Unit = m.Unit,
                StockQuantity = m.Batches != null && m.Batches.Any() ? m.Batches.Where(b => b.ExpiryDate > DateTime.UtcNow).Sum(b => b.CurrentQuantity) : m.StockQuantity
            }));

            return list;
        }

        public async Task<IEnumerable<InvoiceDto>> GetCustomerInvoicesAsync(System.Guid customerId)
        {
            var invoicesList = await _unitOfWork.Invoices.FindWithIncludesAsync(
                i => i.Appointment != null && i.Appointment.CustomerId == customerId,
                i => i.InvoiceItems,
                i => i.Appointment!.Customer!,
                i => i.Appointment!.Pet!,
                i => i.Appointment!.Doctor!
            );

            var invoices = invoicesList
                .OrderByDescending(i => i.CreatedAt)
                .ToList();

            return invoices.Select(MapToDto).ToList();
        }

        private async Task<InvoiceDto> GetInvoiceWithDetailsAsync(long invoiceId)
        {
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.Id == invoiceId,
                i => i.InvoiceItems,
                i => i.Appointment!.Customer!,
                i => i.Appointment!.Pet!,
                i => i.Appointment!.Doctor!
            );
            if (invoice == null) throw new InvalidOperationException("Không tìm thấy.");

            return MapToDto(invoice);
        }

        public async Task<bool> ProcessSePayWebhookAsync(SePayWebhookDto payload)
        {
            if (string.IsNullOrEmpty(payload.Content)) return false;

            // Dùng Regex tìm MPC + Id hóa đơn (VD: MPC12345)
            var match = System.Text.RegularExpressions.Regex.Match(payload.Content, @"MPC(\d+)");
            if (!match.Success) return false;

            if (!long.TryParse(match.Groups[1].Value, out long invoiceId)) return false;

            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(
                i => i.Id == invoiceId,
                i => i.Appointment!,
                i => i.InvoiceItems
            );

            if (invoice == null) return false;
            
            // Nếu đã thanh toán rồi thì thôi (trường hợp webhook gọi lại)
            if (invoice.PaymentStatus == "paid") return true;

            // Kiểm tra số tiền chuyển phải >= tổng cần thanh toán (TotalAmount)
            if (payload.TransferAmount < invoice.TotalAmount)
            {
                // Có thể lưu log ở đây nếu khách chuyển thiếu
                return false;
            }

            // Thanh toán thành công
            invoice.PaymentStatus = "paid";
            invoice.PaymentMethod = "VietQR";
            invoice.PaidAt = DateTime.UtcNow;

            if (invoice.Appointment != null)
            {
                invoice.Appointment.Status = "completed";
                invoice.Appointment.CheckOutTime = DateTime.UtcNow;
            }

            // Trừ kho
            var invoiceItems = await _unitOfWork.InvoiceItems.FindAsync(
                ii => ii.InvoiceId == invoiceId && ii.ItemType == "medicine"
            );

            foreach (var item in invoiceItems)
            {
                if (item.ItemId.HasValue)
                {
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.ItemId.Value);
                    if (medicine != null)
                    {
                        medicine.StockQuantity -= item.Quantity;
                        if (medicine.StockQuantity < 0) medicine.StockQuantity = 0;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Gửi email
            try
            {
                var customerId = invoice.Appointment?.CustomerId;
                if (customerId.HasValue)
                {
                    var customerUser = _unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == customerId.Value && u.IsActive == true);
                    if (customerUser != null && !string.IsNullOrEmpty(customerUser.Email))
                    {
                        string invoiceCode = "INV-" + invoice.Id.ToString("D5");
                        var itemsDto = invoice.InvoiceItems?.Select(ii => (ii.ItemName ?? "", ii.Quantity, ii.TotalPrice)) 
                                       ?? new List<(string, int, decimal)>();

                        var emailHtml = MyPetClinic.Application.Utils.EmailTemplateBuilder.BuildThankYouInvoiceEmail(
                            customerName: customerUser.FullName ?? invoice.Appointment?.Customer?.FullName ?? "Khách hàng",
                            invoiceCode: invoiceCode,
                            totalAmount: invoice.TotalAmount,
                            petName: invoice.Appointment?.Pet?.Name,
                            doctorName: invoice.Appointment?.Doctor?.FullName,
                            appointmentDate: invoice.Appointment?.AppointmentDate,
                            items: itemsDto,
                            discountAmount: invoice.DiscountAmount
                        );
                        await _emailQueue.QueueEmailAsync(new MyPetClinic.Application.DTOs.Notification.EmailMessageDto
                        {
                            ToEmail = customerUser.Email,
                            Subject = $"MyPetClinic - Cảm ơn bạn đã sử dụng dịch vụ ({invoiceCode})",
                            BodyHtml = emailHtml
                        });
                    }
                }
            }
            catch { /* Ignore */ }

            return true;
        }

        private static InvoiceDto MapToDto(Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                AppointmentId = invoice.AppointmentId,
                Subtotal = invoice.Subtotal,
                DiscountAmount = invoice.DiscountAmount,
                TotalAmount = invoice.TotalAmount,
                PaymentStatus = invoice.PaymentStatus,
                PaymentMethod = invoice.PaymentMethod,
                PaidAt = invoice.PaidAt,
                CreatedAt = invoice.CreatedAt,
                AppointmentDate = invoice.Appointment?.AppointmentDate,
                StartTime = invoice.Appointment?.StartTime,
                CustomerId = invoice.Appointment?.CustomerId,
                CustomerName = invoice.Appointment?.Customer?.FullName ?? "Khách vãng lai",
                CustomerPhone = invoice.Appointment?.Customer?.Phone ?? "",
                PetName = invoice.Appointment?.Pet?.Name ?? "Thú cưng",
                PetSpecies = invoice.Appointment?.Pet?.Species,
                PetBreed = invoice.Appointment?.Pet?.Breed,
                DoctorName = invoice.Appointment?.Doctor?.FullName,
                Items = invoice.InvoiceItems.Select(ii => new InvoiceItemDto
                {
                    Id = ii.Id,
                    InvoiceId = ii.InvoiceId,
                    ItemType = ii.ItemType,
                    ItemId = ii.ItemId,
                    ItemName = ii.ItemName,
                    Quantity = ii.Quantity,
                    UnitPrice = ii.UnitPrice,
                    TotalPrice = ii.TotalPrice
                }).OrderBy(ii => ii.ItemType).ThenBy(ii => ii.ItemName).ToList()
            };
        }
    }
}
