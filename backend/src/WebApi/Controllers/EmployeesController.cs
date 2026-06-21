using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "admin,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var result = await _employeeService.GetEmployeesAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _employeeService.CreateEmployeeAsync(request);
                return Ok(new { success = true, employee = result, message = "Đã thêm nhân viên và gửi email kích hoạt." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _employeeService.UpdateEmployeeAsync(id, request);
                return Ok(new { success = true, employee = result, message = "Cập nhật nhân viên thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/resend-activation")]
        public async Task<IActionResult> ResendActivationEmail(Guid id)
        {
            try
            {
                var result = await _employeeService.ResendActivationEmailAsync(id);
                if (result)
                    return Ok(new { success = true, message = "Đã gửi lại email kích hoạt." });
                else
                    return BadRequest(new { success = false, message = "Không thể gửi lại email kích hoạt (tài khoản đã kích hoạt hoặc không tồn tại)." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
