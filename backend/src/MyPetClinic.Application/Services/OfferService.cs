using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.DTOs.Offer;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfferService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResultDto<OfferDto>> GetOffersAsync(int pageNumber, int pageSize, string? status, string? search)
        {
            IQueryable<Offer> query = _unitOfWork.Offers.Query()
                .Include(o => o.OfferServices);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(lowerSearch) || o.Code.ToLower().Contains(lowerSearch));
            }

            var totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();

            return new PaginatedResultDto<OfferDto>
            {
                Items = dtos,
                TotalCount = totalItems,
                PageIndex = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<PaginatedResultDto<OfferDto>> GetPublicOffersAsync(int pageNumber, int pageSize, Guid? customerId = null)
        {
            var today = DateTime.UtcNow;
            IQueryable<Offer> query = _unitOfWork.Offers.Query()
                .Include(o => o.OfferServices)
                .Where(o => o.IsPublic && o.Status == "ACTIVE" && o.StartDate <= today && o.EndDate >= today)
                .Where(o => o.TotalQuantity == null || o.UsedQuantity < o.TotalQuantity);

            if (customerId.HasValue)
            {
                var userUserId = await GetUserIdFromCustomerIdAsync(customerId.Value);
                
                var usedOfferIds = new List<Guid>();
                if (userUserId.HasValue)
                {
                    // Fetch offers that this user has fully used up
                    usedOfferIds = await _unitOfWork.OfferUsageLogs.Query()
                        .Where(log => log.UserId == userUserId.Value && log.Status == "APPLIED")
                        .Select(log => log.OfferId)
                        .ToListAsync();
                }

                // Fetch ALL appointments for this customer that are not cancelled and have a note
                var customerAppointments = await _unitOfWork.Appointments.Query()
                    .Where(a => a.CustomerId == customerId.Value && a.Status != "cancelled" && a.Note != null)
                    .Select(a => new { a.Status, a.Note })
                    .ToListAsync();

                var offerLimits = await query.Select(o => new { o.Id, o.Code, o.UsageLimitPerUser }).ToListAsync();
                
                var exhaustedOfferIds = new List<Guid>();
                foreach (var offerLimit in offerLimits)
                {
                    var appliedLogCount = usedOfferIds.Count(id => id == offerLimit.Id);
                    
                    var voucherString = $"[Áp dụng voucher: {offerLimit.Code}]";
                    
                    // Count ALL appointments (both completed and pending) that have this voucher code in their Note.
                    // We use OrdinalIgnoreCase just to be absolutely safe.
                    var appointmentNoteCount = customerAppointments
                        .Count(a => a.Note != null && a.Note.Contains(voucherString, StringComparison.OrdinalIgnoreCase));

                    // Some completed appointments might not have an OfferUsageLog if the receptionist bypassed it.
                    // To prevent reuse, we take the maximum between the formal logs and the note traces.
                    var totalUsages = Math.Max(appliedLogCount, appointmentNoteCount);

                    if (totalUsages >= offerLimit.UsageLimitPerUser)
                    {
                        exhaustedOfferIds.Add(offerLimit.Id);
                    }
                }

                if (exhaustedOfferIds.Any())
                {
                    query = query.Where(o => !exhaustedOfferIds.Contains(o.Id));
                }
            }

            var totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();

            return new PaginatedResultDto<OfferDto>
            {
                Items = dtos,
                TotalCount = totalItems,
                PageIndex = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<OfferDto> GetOfferByIdAsync(Guid id)
        {
            var offer = await _unitOfWork.Offers.Query()
                .Include(o => o.OfferServices)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (offer == null) throw new InvalidOperationException("Không tìm thấy mã giảm giá.");
            return MapToDto(offer);
        }

        public async Task<OfferDto> CreateOfferAsync(CreateOfferDto dto, Guid currentUserId)
        {
            var existingCode = await _unitOfWork.Offers.Query().AnyAsync(o => o.Code == dto.Code);
            if (existingCode) throw new InvalidOperationException("Mã giảm giá đã tồn tại.");

            if (dto.StartDate >= dto.EndDate) throw new InvalidOperationException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            var offer = new Offer
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MaxDiscount = dto.MaxDiscount,
                MinOrderValue = dto.MinOrderValue,
                TotalQuantity = dto.TotalQuantity,
                UsageLimitPerUser = dto.UsageLimitPerUser,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsPublic = dto.IsPublic,
                CreatedBy = currentUserId,
                Status = "ACTIVE"
            };

            await _unitOfWork.Offers.AddAsync(offer);

            if (dto.AppliedServiceIds != null && dto.AppliedServiceIds.Any())
            {
                foreach (var serviceId in dto.AppliedServiceIds)
                {
                    await _unitOfWork.OfferServices.AddAsync(new MyPetClinic.Domain.Entities.OfferService
                    {
                        OfferId = offer.Id,
                        ServiceId = serviceId
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetOfferByIdAsync(offer.Id);
        }

        public async Task<OfferDto> UpdateOfferAsync(Guid id, UpdateOfferDto dto)
        {
            var offer = await _unitOfWork.Offers.Query()
                .Include(o => o.OfferServices)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (offer == null) throw new InvalidOperationException("Không tìm thấy mã giảm giá.");

            offer.Name = dto.Name;
            offer.Description = dto.Description;
            offer.Status = string.IsNullOrEmpty(dto.Status) ? offer.Status : dto.Status;
            offer.TotalQuantity = dto.TotalQuantity;
            offer.EndDate = dto.EndDate;
            offer.IsPublic = dto.IsPublic;
            offer.UpdatedAt = DateTime.UtcNow;

            // Update Applied Services
            var currentOfferServices = offer.OfferServices.ToList();
            foreach (var os in currentOfferServices)
            {
                _unitOfWork.OfferServices.Remove(os);
            }
            
            if (dto.AppliedServiceIds != null && dto.AppliedServiceIds.Any())
            {
                foreach (var serviceId in dto.AppliedServiceIds)
                {
                    await _unitOfWork.OfferServices.AddAsync(new MyPetClinic.Domain.Entities.OfferService
                    {
                        OfferId = offer.Id,
                        ServiceId = serviceId
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetOfferByIdAsync(offer.Id);
        }

        public async Task LockOfferAsync(Guid id)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id);
            if (offer == null) throw new InvalidOperationException("Không tìm thấy mã giảm giá.");

            offer.Status = "LOCKED";
            offer.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ValidateOfferResponseDto> ValidateOfferAsync(ValidateOfferRequestDto request, Guid? customerId)
        {
            var offer = await _unitOfWork.Offers.Query()
                .Include(o => o.OfferServices)
                .FirstOrDefaultAsync(o => o.Code == request.Code);

            if (offer == null) return new ValidateOfferResponseDto { IsValid = false, Message = "Mã giảm giá không tồn tại." };
            
            if (offer.Status != "ACTIVE") return new ValidateOfferResponseDto { IsValid = false, Message = "Mã giảm giá đang bị khóa." };
            
            var today = DateTime.UtcNow;
            if (offer.StartDate > today) return new ValidateOfferResponseDto { IsValid = false, Message = "Mã giảm giá chưa đến ngày áp dụng." };
            if (offer.EndDate < today) return new ValidateOfferResponseDto { IsValid = false, Message = "Mã giảm giá đã hết hạn." };

            if (offer.TotalQuantity.HasValue && offer.UsedQuantity >= offer.TotalQuantity.Value)
            {
                return new ValidateOfferResponseDto { IsValid = false, Message = "Mã giảm giá đã hết lượt sử dụng." };
            }

            if (request.OrderAmount < offer.MinOrderValue)
            {
                return new ValidateOfferResponseDto { IsValid = false, Message = $"Đơn hàng chưa đạt giá trị tối thiểu {offer.MinOrderValue:N0}đ." };
            }

            if (offer.OfferServices.Any())
            {
                var requiredServiceIds = offer.OfferServices.Select(os => os.ServiceId).ToList();
                if (!request.ServiceIds.Any(sid => requiredServiceIds.Contains(sid)))
                {
                    return new ValidateOfferResponseDto { IsValid = false, Message = "Mã giảm giá không áp dụng cho các dịch vụ trong đơn hàng này." };
                }
            }

            if (customerId.HasValue)
            {
                var userUserId = await GetUserIdFromCustomerIdAsync(customerId.Value);
                
                int appliedCount = 0;
                if (userUserId.HasValue)
                {
                    appliedCount = await _unitOfWork.OfferUsageLogs.Query()
                        .CountAsync(log => log.OfferId == offer.Id && log.UserId == userUserId.Value && log.Status == "APPLIED");
                }
                
                var customerAppointments = await _unitOfWork.Appointments.Query()
                    .Where(a => a.CustomerId == customerId.Value && a.Status != "cancelled" && a.Note != null)
                    .Select(a => new { a.Status, a.Note })
                    .ToListAsync();
                    
                var voucherString = $"[Áp dụng voucher: {offer.Code}]";
                var appointmentNoteCount = customerAppointments
                    .Count(a => a.Note != null && a.Note.Contains(voucherString, StringComparison.OrdinalIgnoreCase));

                var totalUsages = Math.Max(appliedCount, appointmentNoteCount);

                if (totalUsages >= offer.UsageLimitPerUser)
                {
                    return new ValidateOfferResponseDto { IsValid = false, Message = "Bạn đã hết lượt sử dụng mã này." };
                }
            }

            decimal discountAmount = 0;
            if (offer.DiscountType == "PERCENTAGE")
            {
                discountAmount = request.OrderAmount * (offer.DiscountValue / 100);
                if (offer.MaxDiscount.HasValue && discountAmount > offer.MaxDiscount.Value)
                {
                    discountAmount = offer.MaxDiscount.Value;
                }
            }
            else
            {
                discountAmount = offer.DiscountValue;
            }

            if (discountAmount > request.OrderAmount) discountAmount = request.OrderAmount;

            return new ValidateOfferResponseDto
            {
                IsValid = true,
                DiscountAmount = discountAmount,
                Message = "Mã giảm giá hợp lệ.",
                Offer = MapToDto(offer)
            };
        }

        public async Task<bool> ApplyOfferAsync(Guid offerId, Guid customerId, long invoiceId, decimal discountApplied)
        {
            var userUserId = await GetUserIdFromCustomerIdAsync(customerId);
            if (!userUserId.HasValue) throw new InvalidOperationException("Khách hàng không tồn tại User account.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var offer = await _unitOfWork.Offers.GetByIdAsync(offerId);
                if (offer == null || offer.Status != "ACTIVE") throw new InvalidOperationException("Mã giảm giá không hợp lệ.");

                if (offer.TotalQuantity.HasValue && offer.UsedQuantity >= offer.TotalQuantity.Value)
                    throw new InvalidOperationException("Mã giảm giá đã hết lượt sử dụng.");

                offer.UsedQuantity += 1;

                var usageLog = new OfferUsageLog
                {
                    OfferId = offerId,
                    UserId = userUserId.Value,
                    InvoiceId = invoiceId,
                    DiscountApplied = discountApplied,
                    Status = "APPLIED"
                };

                await _unitOfWork.OfferUsageLogs.AddAsync(usageLog);
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> RevertOfferAsync(Guid offerUsageLogId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var usageLog = await _unitOfWork.OfferUsageLogs.GetByIdAsync(offerUsageLogId);
                if (usageLog == null || usageLog.Status != "APPLIED") return false;

                var offer = await _unitOfWork.Offers.GetByIdAsync(usageLog.OfferId);
                if (offer != null)
                {
                    offer.UsedQuantity -= 1;
                    if (offer.UsedQuantity < 0) offer.UsedQuantity = 0;
                }

                usageLog.Status = "REVERTED";
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<Guid?> GetUserIdFromCustomerIdAsync(Guid customerId)
        {
            var user = await _unitOfWork.Users.Query().FirstOrDefaultAsync(u => u.CustomerId == customerId);
            return user?.Id;
        }

        private static OfferDto MapToDto(Offer o)
        {
            return new OfferDto
            {
                Id = o.Id,
                Code = o.Code,
                Name = o.Name,
                Description = o.Description,
                DiscountType = o.DiscountType,
                DiscountValue = o.DiscountValue,
                MaxDiscount = o.MaxDiscount,
                MinOrderValue = o.MinOrderValue,
                TotalQuantity = o.TotalQuantity,
                UsedQuantity = o.UsedQuantity,
                UsageLimitPerUser = o.UsageLimitPerUser,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                Status = o.Status,
                IsPublic = o.IsPublic,
                CreatedAt = o.CreatedAt,
                AppliedServiceIds = o.OfferServices.Select(os => os.ServiceId).ToList()
            };
        }
    }
}
