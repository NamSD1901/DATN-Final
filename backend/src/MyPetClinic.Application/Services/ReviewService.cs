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
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResultDto<ReviewDto>> GetReviewsAsync(int page, int limit, string? sortBy, short? rating, string? petType, long? serviceId, Guid? doctorId, bool? hasImages, bool includeDeleted = false)
        {
            var query = _unitOfWork.Reviews.Query()
                .Include(r => r.Customer)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Pet)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Doctor)
                .AsQueryable();

            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(r => r.DeletedAt == null);
            }

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }
            if (!string.IsNullOrEmpty(petType))
            {
                query = query.Where(r => r.Appointment != null && r.Appointment.Pet != null && r.Appointment.Pet.Species == petType);
            }
            if (serviceId.HasValue)
            {
                query = query.Where(r => r.Appointment != null && r.Appointment.ServiceId == serviceId.Value);
            }
            if (doctorId.HasValue)
            {
                query = query.Where(r => r.Appointment != null && r.Appointment.DoctorId == doctorId.Value);
            }
            if (hasImages.HasValue && hasImages.Value)
            {
                query = query.Where(r => !string.IsNullOrEmpty(r.ImageUrls));
            }

            query = sortBy switch
            {
                "rating_asc" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                "rating_desc" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                "date_asc" => query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt) // default date_desc
            };

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var dtos = items.Select(r => new ReviewDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer?.FullName ?? "Unknown",
                CustomerAvatarUrl = r.Customer?.Avatar,
                AppointmentId = r.AppointmentId,
                ServiceName = r.Appointment?.Service?.Name,
                DoctorId = r.Appointment?.DoctorId,
                DoctorName = r.Appointment?.Doctor?.FullName,
                PetName = r.Appointment?.Pet?.Name,
                PetBreed = r.Appointment?.Pet?.Breed,
                PetAge = r.Appointment?.Pet?.BirthDate.HasValue == true ? Math.Floor((DateTime.UtcNow - r.Appointment.Pet.BirthDate.Value).TotalDays / 365.25).ToString() : null,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                DeletedAt = r.DeletedAt,
                IsVerified = r.IsVerified,
                ClinicReply = r.ClinicReply,
                RepliedAt = r.RepliedAt,
                HelpfulCount = r.HelpfulCount,
                LikeCount = r.LikeCount,
                ImageUrls = r.ImageUrls
            }).ToList();

            return new PaginatedResultDto<ReviewDto>(dtos, totalItems, page, limit);
        }

        public async Task<PaginatedResultDto<ReviewDto>> GetMyReviewsAsync(Guid customerId, int page, int limit)
        {
            var query = _unitOfWork.Reviews.Query()
                .Include(r => r.Appointment)
                .ThenInclude(a => a.Service)
                .Where(r => r.CustomerId == customerId && r.DeletedAt == null)
                .OrderByDescending(r => r.CreatedAt);

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var dtos = items.Select(r => new ReviewDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                AppointmentId = r.AppointmentId,
                ServiceName = r.Appointment?.Service?.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                DeletedAt = r.DeletedAt
            }).ToList();

            return new PaginatedResultDto<ReviewDto>(dtos, totalItems, page, limit);
        }

        public async Task<ReviewDto> GetReviewByIdAsync(long id)
        {
            var review = await _unitOfWork.Reviews.Query()
                .Include(r => r.Customer)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Pet)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Doctor)
                .IgnoreQueryFilters() // Admins might need to fetch a specific deleted review
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đánh giá với Id: {id}");
            }

            return new ReviewDto
            {
                Id = review.Id,
                CustomerId = review.CustomerId,
                CustomerName = review.Customer?.FullName ?? "Unknown",
                CustomerAvatarUrl = review.Customer?.Avatar,
                AppointmentId = review.AppointmentId,
                ServiceName = review.Appointment?.Service?.Name,
                DoctorId = review.Appointment?.DoctorId,
                DoctorName = review.Appointment?.Doctor?.FullName,
                PetName = review.Appointment?.Pet?.Name,
                PetBreed = review.Appointment?.Pet?.Breed,
                PetAge = review.Appointment?.Pet?.BirthDate.HasValue == true ? Math.Floor((DateTime.UtcNow - review.Appointment.Pet.BirthDate.Value).TotalDays / 365.25).ToString() : null,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                DeletedAt = review.DeletedAt,
                IsVerified = review.IsVerified,
                ClinicReply = review.ClinicReply,
                RepliedAt = review.RepliedAt,
                HelpfulCount = review.HelpfulCount,
                LikeCount = review.LikeCount,
                ImageUrls = review.ImageUrls
            };
        }

        public async Task<ReviewDto> CreateReviewAsync(Guid customerId, CreateReviewDto dto)
        {
            var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                a => a.Id == dto.AppointmentId && a.CustomerId == customerId,
                a => a.Service);

            if (appointment == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy lịch hẹn của bạn với Id: {dto.AppointmentId}");
            }

            if (appointment.Status != "completed")
            {
                throw new InvalidOperationException("Bạn chỉ có thể đánh giá sau khi lịch hẹn đã hoàn tất (Completed).");
            }

            var baseDate = appointment.CheckOutTime ?? appointment.AppointmentDate;
            var daysSinceCompleted = (DateTime.UtcNow - baseDate).TotalDays;
            if (daysSinceCompleted > 14)
            {
                throw new InvalidOperationException("Bạn chỉ được đánh giá trong vòng 14 ngày kể từ khi hoàn tất dịch vụ.");
            }

            var existingReview = await _unitOfWork.Reviews.AnyAsync(r => r.AppointmentId == dto.AppointmentId);
            if (existingReview)
            {
                throw new InvalidOperationException("Lịch hẹn này đã được đánh giá.");
            }

            var review = new Review
            {
                CustomerId = customerId,
                AppointmentId = dto.AppointmentId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);

            return new ReviewDto
            {
                Id = review.Id,
                CustomerId = review.CustomerId,
                CustomerName = customer?.FullName ?? "Unknown",
                CustomerAvatarUrl = customer?.Avatar,
                AppointmentId = review.AppointmentId,
                ServiceName = appointment.Service?.Name,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<ReviewDto> UpdateReviewAsync(Guid customerId, long id, UpdateReviewDto dto)
        {
            var review = await _unitOfWork.Reviews.GetFirstOrDefaultWithIncludesAsync(
                r => r.Id == id && r.CustomerId == customerId && r.DeletedAt == null,
                r => r.Customer, r => r.Appointment);

            if (review == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đánh giá của bạn với Id: {id}");
            }

            var daysSinceCreated = (DateTime.UtcNow - review.CreatedAt).TotalDays;
            if (daysSinceCreated > 7)
            {
                throw new InvalidOperationException("Bạn chỉ được sửa đánh giá trong vòng 7 ngày kể từ khi tạo.");
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(a => a.Id == review.AppointmentId, a => a.Service);

            return new ReviewDto
            {
                Id = review.Id,
                CustomerId = review.CustomerId,
                CustomerName = review.Customer?.FullName ?? "Unknown",
                CustomerAvatarUrl = review.Customer?.Avatar,
                AppointmentId = review.AppointmentId,
                ServiceName = appointment?.Service?.Name,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                DeletedAt = review.DeletedAt
            };
        }

        public async Task SoftDeleteReviewAsync(long id)
        {
            var review = await _unitOfWork.Reviews.Query().IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đánh giá với Id: {id}");
            }

            if (review.DeletedAt != null)
            {
                throw new InvalidOperationException("Đánh giá này đã bị xóa mềm từ trước.");
            }

            review.DeletedAt = DateTime.UtcNow;
            
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RestoreReviewAsync(long id)
        {
            var review = await _unitOfWork.Reviews.Query().IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đánh giá với Id: {id}");
            }

            if (review.DeletedAt == null)
            {
                throw new InvalidOperationException("Đánh giá này không bị xóa mềm, không thể khôi phục.");
            }

            review.DeletedAt = null;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync()
        {
            var query = _unitOfWork.Reviews.Query().IgnoreQueryFilters().AsQueryable();

            var total = await query.CountAsync();
            var avg = total > 0 ? await query.AverageAsync(r => (double)r.Rating) : 0;
            
            var distribution = await query
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            var result = new ReviewStatisticsDto
            {
                Total = total,
                AvgRating = Math.Round(avg, 1)
            };

            for (short i = 1; i <= 5; i++)
            {
                result.RatingDistribution[i] = distribution.ContainsKey(i) ? distribution[i] : 0;
            }

            return result;
        }

        public async Task IncrementHelpfulCountAsync(long id)
        {
            var review = await _unitOfWork.Reviews.Query().IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đánh giá với Id: {id}");
            }

            review.HelpfulCount++;
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DecrementHelpfulCountAsync(long id)
        {
            var review = await _unitOfWork.Reviews.Query().IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đánh giá với Id: {id}");
            }

            if (review.HelpfulCount > 0)
            {
                review.HelpfulCount--;
                _unitOfWork.Reviews.Update(review);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
