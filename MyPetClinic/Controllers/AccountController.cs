using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Models;
using BCrypt.Net;

namespace MyPetClinic.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 1. Kiểm tra Email đã tồn tại
        var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
        if (emailExists)
        {
            ModelState.AddModelError("Email", "Email này đã được đăng ký trong hệ thống.");
            return View(model);
        }

        // 2. Lấy role_id cho 'customer' (Mặc định là 4 theo database.sql)
        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "customer");
        long roleId = customerRole?.Id ?? 4;

        // 3. Mã hóa mật khẩu sử dụng BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

        // 4. Tạo thực thể User mới
        var user = new User
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            PasswordHash = passwordHash,
            Gender = model.Gender,
            DateOfBirth = model.DateOfBirth?.ToUniversalTime(),
            Address = model.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Lưu vào Database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 6. Chuyển hướng sang trang hiển thị thành công demo
        return RedirectToAction(nameof(RegisterSuccess), new { id = user.Id });
    }

    [HttpGet]
    public async Task<IActionResult> RegisterSuccess(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return RedirectToAction(nameof(Register));
        }

        return View(user);
    }
}
