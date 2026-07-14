using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class BlockTimeService : IBlockTimeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BlockTimeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BlockTimeDto>> GetBlockTimesAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, Guid? doctorId)
        {
            var query = _unitOfWork.BlockTimes.Query();

            if (startDate.HasValue)
            {
                query = query.Where(b => b.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.EndTime <= endDate.Value);
            }

            if (doctorId.HasValue)
            {
                query = query.Where(b => b.DoctorId == doctorId.Value);
            }

            var blocks = query.ToList();
            var doctorIds = blocks.Select(b => b.DoctorId).Distinct().ToList();
            
            var doctors = _unitOfWork.Users.Query()
                .Where(u => doctorIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName ?? string.Empty);

            return blocks.OrderBy(b => b.StartTime).Select(b => new BlockTimeDto
            {
                Id = b.Id,
                DoctorId = b.DoctorId,
                DoctorName = doctors.TryGetValue(b.DoctorId, out var name) ? name : string.Empty,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                BlockType = b.BlockType,
                Reason = b.Reason,
                BackgroundColor = b.BackgroundColor
            });
        }

        public async Task<BlockTimeDto?> GetBlockTimeByIdAsync(Guid id)
        {
            var block = await _unitOfWork.BlockTimes.GetByIdAsync(id);
            if (block == null) return null;

            var user = await _unitOfWork.Users.GetByIdAsync(block.DoctorId);

            return new BlockTimeDto
            {
                Id = block.Id,
                DoctorId = block.DoctorId,
                DoctorName = user?.FullName ?? string.Empty,
                StartTime = block.StartTime,
                EndTime = block.EndTime,
                BlockType = block.BlockType,
                Reason = block.Reason,
                BackgroundColor = block.BackgroundColor
            };
        }

        public async Task<Guid> CreateBlockTimeAsync(BlockTimeCreateDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            if (dto.StartTime.Date < DateTimeOffset.UtcNow.Date)
            {
                throw new InvalidOperationException("Không được tạo Block Time trong ngày quá khứ.");
            }

            var block = new BlockTime
            {
                DoctorId = dto.DoctorId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                BlockType = dto.BlockType,
                Reason = dto.Reason,
                BackgroundColor = dto.BackgroundColor
            };

            await _unitOfWork.BlockTimes.AddAsync(block);
            await _unitOfWork.SaveChangesAsync();

            return block.Id;
        }

        public async Task<bool> UpdateBlockTimeAsync(Guid id, BlockTimeUpdateDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            var block = await _unitOfWork.BlockTimes.GetByIdAsync(id);
            if (block == null) return false;

            if (block.StartTime.Date < DateTimeOffset.UtcNow.Date)
            {
                throw new InvalidOperationException("Không được cập nhật Block Time trong ngày quá khứ.");
            }

            block.StartTime = dto.StartTime;
            block.EndTime = dto.EndTime;
            block.BlockType = dto.BlockType;
            block.Reason = dto.Reason;
            block.BackgroundColor = dto.BackgroundColor;

            _unitOfWork.BlockTimes.Update(block);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBlockTimeAsync(Guid id)
        {
            var block = await _unitOfWork.BlockTimes.GetByIdAsync(id);
            if (block == null) return false;

            _unitOfWork.BlockTimes.Remove(block);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
