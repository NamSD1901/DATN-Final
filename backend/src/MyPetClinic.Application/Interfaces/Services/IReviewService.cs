using MyPetClinic.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<PaginatedResultDto<ReviewDto>> GetReviewsAsync(int page, int limit, string? sortBy, short? rating, bool includeDeleted = false);
        Task<PaginatedResultDto<ReviewDto>> GetMyReviewsAsync(Guid customerId, int page, int limit);
        Task<ReviewDto> GetReviewByIdAsync(long id);
        Task<ReviewDto> CreateReviewAsync(Guid customerId, CreateReviewDto dto);
        Task<ReviewDto> UpdateReviewAsync(Guid customerId, long id, UpdateReviewDto dto);
        Task SoftDeleteReviewAsync(long id);
        Task RestoreReviewAsync(long id);
        Task<ReviewStatisticsDto> GetReviewStatisticsAsync();
    }
}
