using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize(Roles = "admin,Admin")]
    [ApiController]
    [Route("api/block-times")]
    public class BlockTimesController : ControllerBase
    {
        private readonly IBlockTimeService _blockTimeService;

        public BlockTimesController(IBlockTimeService blockTimeService)
        {
            _blockTimeService = blockTimeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlockTimes(
            [FromQuery] DateTimeOffset? startDate, 
            [FromQuery] DateTimeOffset? endDate, 
            [FromQuery] Guid? doctorId)
        {
            var blocks = await _blockTimeService.GetBlockTimesAsync(startDate, endDate, doctorId);
            return Ok(blocks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlockTimeById(Guid id)
        {
            var block = await _blockTimeService.GetBlockTimeByIdAsync(id);
            if (block == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin Block Time." });
            }
            return Ok(block);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlockTime([FromBody] BlockTimeCreateDto dto)
        {
            try
            {
                var id = await _blockTimeService.CreateBlockTimeAsync(dto);
                return Ok(new { success = true, id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlockTime(Guid id, [FromBody] BlockTimeUpdateDto dto)
        {
            try
            {
                var success = await _blockTimeService.UpdateBlockTimeAsync(id, dto);
                if (!success)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin Block Time." });
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlockTime(Guid id)
        {
            try
            {
                var success = await _blockTimeService.DeleteBlockTimeAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin Block Time." });
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
