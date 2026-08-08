using System;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.DTOs.Offer;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IOfferService
    {
        Task<PaginatedResultDto<OfferDto>> GetOffersAsync(int pageNumber, int pageSize, string? status, string? search);
        Task<PaginatedResultDto<OfferDto>> GetPublicOffersAsync(int pageNumber, int pageSize, Guid? customerId = null);
        Task<OfferDto> GetOfferByIdAsync(Guid id);
        Task<OfferDto> CreateOfferAsync(CreateOfferDto dto, Guid currentUserId);
        Task<OfferDto> UpdateOfferAsync(Guid id, UpdateOfferDto dto);
        Task LockOfferAsync(Guid id);
        
        Task<ValidateOfferResponseDto> ValidateOfferAsync(ValidateOfferRequestDto request, Guid? customerId);
        
        Task<bool> ApplyOfferAsync(Guid offerId, Guid customerId, long invoiceId, decimal discountApplied);
        Task<bool> RevertOfferAsync(Guid offerUsageLogId);
    }
}
