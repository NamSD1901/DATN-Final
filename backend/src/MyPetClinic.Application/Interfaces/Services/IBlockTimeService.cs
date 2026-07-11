using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IBlockTimeService
    {
        Task<IEnumerable<BlockTimeDto>> GetBlockTimesAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, Guid? doctorId);
        Task<BlockTimeDto?> GetBlockTimeByIdAsync(Guid id);
        Task<Guid> CreateBlockTimeAsync(BlockTimeCreateDto dto);
        Task<bool> UpdateBlockTimeAsync(Guid id, BlockTimeUpdateDto dto);
        Task<bool> DeleteBlockTimeAsync(Guid id);
    }
}
