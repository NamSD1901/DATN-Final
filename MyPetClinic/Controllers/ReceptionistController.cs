using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "Receptionist,Admin")]
    public class ReceptionistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceptionistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH KHÁCH HÀNG (có tìm kiếm)
        // ==========================================
        public async Task<IActionResult> Customers(string? search)
        {
            ViewBag.Search = search;

            // Lấy Role "customer"
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "customer");
            if (customerRole == null)
                return View(new List<User>());

            var query = _context.Users
                .Where(u => u.RoleId == customerRole.Id && u.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(keyword)) ||
                    (u.Phone != null && u.Phone.Contains(keyword)) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword))
                );
            }

            var customers = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(customers);
        }

        // ==========================================
        // 2. CHI TIẾT KHÁCH HÀNG + DANH SÁCH THÚ CƯNG
        // ==========================================
        public async Task<IActionResult> CustomerDetail(Guid id)
        {
            var customer = await _context.Users.FindAsync(id);
            if (customer == null) return NotFound();

            var pets = await _context.Pets
                .Where(p => p.OwnerId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Customer = customer;
            ViewBag.Pets = pets;
            return View();
        }

        // ==========================================
        // 3. THÊM KHÁCH HÀNG MỚI
        // ==========================================
        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(User model)
        {
            // Kiểm tra SĐT hoặc Email trùng
            if (!string.IsNullOrEmpty(model.Phone))
            {
                var existPhone = await _context.Users.AnyAsync(u => u.Phone == model.Phone);
                if (existPhone)
                {
                    TempData["Error"] = "Số điện thoại này đã tồn tại trong hệ thống.";
                    return View(model);
                }
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                var existEmail = await _context.Users.AnyAsync(u => u.Email == model.Email.Trim().ToLower());
                if (existEmail)
                {
                    TempData["Error"] = "Email này đã tồn tại trong hệ thống.";
                    return View(model);
                }
            }

            // Lấy Role "customer"
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "customer");
            if (customerRole == null)
            {
                TempData["Error"] = "Không tìm thấy Role Customer trong hệ thống.";
                return View(model);
            }

            // Tạo mật khẩu ngẫu nhiên tạm thời
            string tempPassword = BCrypt.Net.BCrypt.HashPassword("123456");

            var newUser = new User
            {
                FullName = model.FullName?.Trim(),
                Email = model.Email?.Trim().ToLower(),
                Phone = model.Phone?.Trim(),
                Address = model.Address?.Trim(),
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                RoleId = customerRole.Id,
                PasswordHash = tempPassword,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo hồ sơ cho khách hàng {newUser.FullName} thành công!";
            return RedirectToAction("CustomerDetail", new { id = newUser.Id });
        }

        // ==========================================
        // 4. THÊM THÚ CƯNG CHO KHÁCH HÀNG
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> AddPet(Guid customerId)
        {
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null) return NotFound();
            ViewBag.Customer = customer;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPet(Pet model, Guid customerId)
        {
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null) return NotFound();

            var newPet = new Pet
            {
                OwnerId = customerId,
                Name = model.Name?.Trim(),
                Species = model.Species?.Trim(),
                Breed = model.Breed?.Trim(),
                Gender = model.Gender,
                BirthDate = model.BirthDate,
                Weight = model.Weight,
                Color = model.Color?.Trim(),
                AllergyNote = model.AllergyNote?.Trim(),
                Sterilized = model.Sterilized,
                MicrochipCode = model.MicrochipCode?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Pets.Add(newPet);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm thú cưng '{newPet.Name}' thành công!";
            return RedirectToAction("CustomerDetail", new { id = customerId });
        }

        // ==========================================
        // 5. SỬA THÔNG TIN THÚ CƯNG
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> EditPet(long id)
        {
            var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
            if (pet == null) return NotFound();
            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(Pet model)
        {
            var pet = await _context.Pets.FindAsync(model.Id);
            if (pet == null) return NotFound();

            pet.Name = model.Name?.Trim();
            pet.Species = model.Species?.Trim();
            pet.Breed = model.Breed?.Trim();
            pet.Gender = model.Gender;
            pet.BirthDate = model.BirthDate;
            pet.Weight = model.Weight;
            pet.Color = model.Color?.Trim();
            pet.AllergyNote = model.AllergyNote?.Trim();
            pet.Sterilized = model.Sterilized;
            pet.MicrochipCode = model.MicrochipCode?.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật thông tin thú cưng '{pet.Name}' thành công!";
            return RedirectToAction("CustomerDetail", new { id = pet.OwnerId });
        }

        // ==========================================
        // 6. TÌM KIẾM NHANH (AJAX) - QR Scan hỗ trợ
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> QuickSearch(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Json(new { found = false });

            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Phone == phone.Trim() && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null)
                return Json(new { found = false });

            var pets = await _context.Pets
                .Where(p => p.OwnerId == user.Id)
                .Select(p => new { p.Id, p.Name, p.Species, p.Breed, p.Weight })
                .ToListAsync();

            return Json(new
            {
                found = true,
                customerId = user.Id,
                fullName = user.FullName,
                phone = user.Phone,
                email = user.Email,
                pets
            });
        }
    }
}
