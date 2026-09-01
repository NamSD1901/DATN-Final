using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IMedicineRepository _medicineRepo;
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slot_config.json");

        public AdminService(IUnitOfWork unitOfWork, IAuditLogService auditLogService, IMedicineRepository medicineRepo)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _medicineRepo = medicineRepo;
        }

        public async Task<object> GetUsersAsync()
        {
            var users = await _unitOfWork.Users.FindWithIncludesAsync(u => u.DeletedAt == null, u => u.Role!);
            return users.Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                role = u.Role?.Name ?? "customer",
                isActive = u.IsActive
            });
        }

        public async Task<IEnumerable<ServiceDto>> GetServicesAsync()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return services.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                CategoryId = s.CategoryId,
                DurationMinutes = s.DurationMinutes,
                Description = s.Description,
                IsActive = s.IsActive
            });
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, string currentUserId)
        {
            var service = new Service
            {
                Name = dto.Name,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                DurationMinutes = dto.DurationMinutes,
                Description = dto.Description,
                IsActive = true
            };

            await _unitOfWork.Services.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateService", $"Tạo dịch vụ mới: {dto.Name}");
            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                CategoryId = service.CategoryId,
                DurationMinutes = service.DurationMinutes,
                Description = service.Description,
                IsActive = service.IsActive
            };
        }

        public async Task<ServiceDto> UpdateServiceAsync(long id, CreateServiceDto dto, string currentUserId)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ.");

            service.Name = dto.Name;
            service.Price = dto.Price;
            service.CategoryId = dto.CategoryId;
            service.DurationMinutes = dto.DurationMinutes;
            service.Description = dto.Description;

            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateService", $"Cập nhật dịch vụ ID {id}: {dto.Name}");
            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                CategoryId = service.CategoryId,
                DurationMinutes = service.DurationMinutes,
                Description = service.Description,
                IsActive = service.IsActive
            };
        }

        public async Task DeleteServiceAsync(long id, string currentUserId)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ.");

            service.IsActive = false;
            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteService", $"Khóa dịch vụ ID {id}");
        }

        public async Task<IEnumerable<ServiceCategoryDto>> GetServiceCategoriesAsync()
        {
            var categories = await _unitOfWork.ServiceCategories.GetAllAsync();
            return categories.Select(c => new ServiceCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            });
        }

        public async Task<ServiceCategoryDto> CreateServiceCategoryAsync(CreateServiceCategoryDto dto, string currentUserId)
        {
            var category = new ServiceCategory
            {
                Name = dto.Name
            };

            await _unitOfWork.ServiceCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateServiceCategory", $"Tạo danh mục dịch vụ mới: {dto.Name}");
            return new ServiceCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<ServiceCategoryDto> UpdateServiceCategoryAsync(long id, CreateServiceCategoryDto dto, string currentUserId)
        {
            var category = await _unitOfWork.ServiceCategories.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy danh mục dịch vụ.");

            category.Name = dto.Name;

            _unitOfWork.ServiceCategories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateServiceCategory", $"Cập nhật danh mục dịch vụ ID {id}: {dto.Name}");
            return new ServiceCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task DeleteServiceCategoryAsync(long id, string currentUserId)
        {
            var category = await _unitOfWork.ServiceCategories.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy danh mục dịch vụ.");

            var services = await _unitOfWork.Services.FindAsync(s => s.CategoryId == id);
            if (services.Any())
            {
                throw new InvalidOperationException("Không thể xoá danh mục đã có dịch vụ.");
            }

            _unitOfWork.ServiceCategories.Remove(category);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteServiceCategory", $"Xoá danh mục dịch vụ ID {id}");
        }

        public async Task<IEnumerable<MedicineDto>> GetMedicinesAsync()
        {
            // Sử dụng GetMedicinesWithStockAsync để Include(Batches)
            // vì StockQuantity là [NotMapped] và được tính từ Batches.Sum(CurrentQuantity)
            var medicines = await _medicineRepo.GetMedicinesWithStockAsync();
            return medicines.Select(m => new MedicineDto
            {
                Id = m.Id,
                Name = m.Name,
                Unit = m.Unit,
                StockQuantity = m.StockQuantity,  // [NotMapped] = Batches.Sum(b => b.CurrentQuantity)
                ImportPrice = m.ImportPrice,
                SellPrice = m.SellPrice,
                ExpiryDate = m.ExpiryDate,
                Batches = m.Batches.Select(b => new MedicineBatchDto
                {
                    Id = b.Id,
                    BatchNumber = b.BatchNumber,
                    MedicineId = b.MedicineId,
                    ManufactureDate = b.ManufactureDate,
                    ExpiryDate = b.ExpiryDate,
                    InitialQuantity = b.InitialQuantity,
                    CurrentQuantity = b.CurrentQuantity
                }).OrderBy(b => b.ExpiryDate).ToList()
            });
        }

        public async Task<object> GetMedicineWarningsAsync()
        {
            // Sử dụng GetMedicinesWithStockAsync để Include(Batches)
            var medicines = await _medicineRepo.GetMedicinesWithStockAsync();
            var today = DateTime.UtcNow.Date;
            var expireThreshold = today.AddDays(30);

            var lowStock = medicines
                .Where(m => m.StockQuantity <= 10)
                .Select(m => new MedicineDto { Id = m.Id, Name = m.Name, Unit = m.Unit, StockQuantity = m.StockQuantity })
                .ToList();
            var expiring = medicines
                .Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date <= expireThreshold)
                .Select(m => new { id = m.Id, name = m.Name, expiryDate = m.ExpiryDate })
                .ToList();

            return new { lowStock, expiring };
        }

        public async Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto dto, string currentUserId)
        {
            var allCategories = await _unitOfWork.MedicineCategories.GetAllAsync();
            long defaultCategoryId = allCategories.FirstOrDefault()?.Id ?? 1;

            // Lưu ý: StockQuantity và ExpiryDate là [NotMapped] trên entity Medicine.
            // Chúng được tính tự động từ MedicineBatches.Sum(CurrentQuantity).
            // Không gán chúng ở đây; tồn kho phải được quản lý thông qua chức năng Nhập Kho.
            var medicine = new Medicine
            {
                Name = dto.Name,
                MedicineCode = "MED-" + DateTime.UtcNow.ToString("yyMMddHHmmss"),
                CategoryId = defaultCategoryId,
                Unit = dto.Unit,
                ImportPrice = dto.ImportPrice,
                SellPrice = dto.SellPrice,
                Description = dto.Description
            };

            await _unitOfWork.Medicines.AddAsync(medicine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateMedicine", $"Tạo thuốc mới: {dto.Name}");
            return new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                Unit = medicine.Unit,
                StockQuantity = 0,  // Tồn kho ban đầu = 0, sẽ tăng khi nhập kho
                ImportPrice = medicine.ImportPrice,
                SellPrice = medicine.SellPrice
            };
        }

        public async Task<MedicineDto> UpdateMedicineAsync(long id, CreateMedicineDto dto, string currentUserId)
        {
            // Dùng GetMedicineWithBatchesAsync để load Batches, đảm bảo StockQuantity được tính đúng
            var medicine = await _medicineRepo.GetMedicineWithBatchesAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thuốc.");

            // Chỉ cập nhật các trường được lưu vào DB.
            // StockQuantity và ExpiryDate là [NotMapped], không cần và không được set ở đây.
            medicine.Name = dto.Name;
            medicine.Unit = dto.Unit;
            medicine.ImportPrice = dto.ImportPrice;
            medicine.SellPrice = dto.SellPrice;
            medicine.Description = dto.Description;

            _unitOfWork.Medicines.Update(medicine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateMedicine", $"Cập nhật thuốc ID {id}: {dto.Name}");
            return new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                Unit = medicine.Unit,
                StockQuantity = medicine.StockQuantity,  // Tính từ Batches (đã Include)
                ImportPrice = medicine.ImportPrice,
                SellPrice = medicine.SellPrice
            };
        }

        public async Task DeleteMedicineAsync(long id, string currentUserId)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thuốc.");

            var batches = await _unitOfWork.MedicineBatches.FindAsync(b => b.MedicineId == id);
            if (batches.Any(b => b.CurrentQuantity > 0))
            {
                throw new InvalidOperationException("Không thể xoá thuốc vẫn còn tồn kho trong các lô.");
            }

            _unitOfWork.Medicines.Remove(medicine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteMedicine", $"Xoá thuốc ID {id}");
        }

        public async Task DeleteMedicineBatchAsync(long batchId, string currentUserId)
        {
            var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(batchId) ?? throw new KeyNotFoundException("Không tìm thấy lô thuốc.");
            
            var transactions = await _unitOfWork.InventoryTransactions.FindAsync(t => t.BatchId == batchId);
            
            // Nếu đã từng xuất kho, không được xoá hẳn mà chỉ xả kho (set tồn = 0)
            if (transactions.Any(t => t.Type != InventoryTransactionType.GoodsReceipt))
            {
                int remainingQty = batch.CurrentQuantity;
                batch.CurrentQuantity = 0;
                _unitOfWork.MedicineBatches.Update(batch);
                
                if (remainingQty > 0)
                {
                    // Sinh phiếu điều chỉnh kho (Adjustment) để sổ sách kế toán kho luôn cân bằng
                    var adjustmentTx = new InventoryTransaction
                    {
                        TransactionDate = DateTime.UtcNow,
                        Type = InventoryTransactionType.Adjustment,
                        MedicineId = batch.MedicineId,
                        BatchId = batch.Id,
                        QuantityChange = -remainingQty,
                        CreatedByUserId = Guid.Parse(currentUserId),
                        Notes = "Thanh lý tự động do xóa lô từ giao diện"
                    };
                    await _unitOfWork.InventoryTransactions.AddAsync(adjustmentTx);
                }
                
                await _auditLogService.LogActionAsync(currentUserId, "DisposeMedicineBatch", $"Xả kho (set 0) lô thuốc ID {batchId} vì đã có giao dịch xuất");
            }
            else
            {
                // Chỉ mới nhập kho (hoặc chưa làm gì), an toàn để xoá
                foreach(var t in transactions) 
                {
                    _unitOfWork.InventoryTransactions.Remove(t);
                }
                _unitOfWork.MedicineBatches.Remove(batch);
                await _auditLogService.LogActionAsync(currentUserId, "DeleteMedicineBatch", $"Xoá lô thuốc ID {batchId}");
            }

            // Trigger medicine update is not strictly needed since StockQuantity is [NotMapped] and dynamically computed
            // but we call SaveChanges
            await _unitOfWork.SaveChangesAsync();
        }

        // ================= VACCINES MANAGEMENT =================
        public async Task<IEnumerable<VaccineAdminDto>> GetVaccinesAsync()
        {
            var vaccines = await _unitOfWork.Vaccines.FindWithIncludesAsync(v => true, v => v.VaccineBatches!);
            
            return vaccines.Select(v => new VaccineAdminDto
            {
                Id = v.Id,
                Name = v.Name,
                Manufacturer = v.Manufacturer,
                Description = v.Description,
                StockQuantity = v.StockQuantity,
                TargetSpecies = v.TargetSpecies,
                MinAgeWeeks = v.MinAgeWeeks,
                IntervalDays = v.IntervalDays,
                Batches = v.VaccineBatches.Select(b => new VaccineBatchAdminDto
                {
                    Id = b.Id,
                    VaccineId = b.VaccineId,
                    BatchNumber = b.BatchNumber,
                    ExpirationDate = b.ExpirationDate,
                    ImportDate = b.ImportDate,
                    StockQuantity = b.StockQuantity,
                    ImportPrice = b.ImportPrice,
                    SellingPrice = b.SellingPrice
                }).OrderBy(b => b.ExpirationDate).ToList()
            }).ToList();
        }

        public async Task<VaccineAdminDto> CreateVaccineAsync(CreateVaccineDto dto, string currentUserId)
        {
            var vaccine = new Vaccine
            {
                Name = dto.Name,
                Manufacturer = dto.Manufacturer,
                Description = dto.Description,
                TargetSpecies = dto.TargetSpecies,
                MinAgeWeeks = dto.MinAgeWeeks,
                IntervalDays = dto.IntervalDays,
                StockQuantity = 0 // Will be updated via batches
            };

            await _unitOfWork.Vaccines.AddAsync(vaccine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateVaccine", $"Thêm loại vắc-xin mới: {dto.Name}");
            return new VaccineAdminDto
            {
                Id = vaccine.Id,
                Name = vaccine.Name,
                Manufacturer = vaccine.Manufacturer,
                Description = vaccine.Description,
                StockQuantity = vaccine.StockQuantity,
                TargetSpecies = vaccine.TargetSpecies,
                MinAgeWeeks = vaccine.MinAgeWeeks,
                IntervalDays = vaccine.IntervalDays
            };
        }

        public async Task<VaccineAdminDto> UpdateVaccineAsync(long id, CreateVaccineDto dto, string currentUserId)
        {
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy vắc-xin.");

            vaccine.Name = dto.Name;
            vaccine.Manufacturer = dto.Manufacturer;
            vaccine.Description = dto.Description;
            vaccine.TargetSpecies = dto.TargetSpecies;
            vaccine.MinAgeWeeks = dto.MinAgeWeeks;
            vaccine.IntervalDays = dto.IntervalDays;

            _unitOfWork.Vaccines.Update(vaccine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateVaccine", $"Cập nhật loại vắc-xin ID {id}: {dto.Name}");
            return new VaccineAdminDto
            {
                Id = vaccine.Id,
                Name = vaccine.Name,
                Manufacturer = vaccine.Manufacturer,
                Description = vaccine.Description,
                StockQuantity = vaccine.StockQuantity,
                TargetSpecies = vaccine.TargetSpecies,
                MinAgeWeeks = vaccine.MinAgeWeeks,
                IntervalDays = vaccine.IntervalDays
            };
        }

        public async Task DeleteVaccineAsync(long id, string currentUserId)
        {
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy vắc-xin.");
            
            // Check if there are remaining batches with stock
            var batches = await _unitOfWork.VaccineBatches.FindAsync(b => b.VaccineId == id);
            if (batches.Any(b => b.StockQuantity > 0))
            {
                throw new InvalidOperationException("Không thể xoá vắc-xin vẫn còn tồn kho trong các lô.");
            }

            _unitOfWork.Vaccines.Remove(vaccine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteVaccine", $"Xoá vắc-xin ID {id}");
        }

        public async Task<VaccineBatchAdminDto> CreateVaccineBatchAsync(long vaccineId, CreateVaccineBatchDto dto, string currentUserId)
        {
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(vaccineId) ?? throw new KeyNotFoundException("Không tìm thấy vắc-xin.");

            var existingBatches = await _unitOfWork.VaccineBatches.FindAsync(b => b.VaccineId == vaccineId && b.BatchNumber == dto.BatchNumber);
            var batch = existingBatches.FirstOrDefault();

            if (batch != null)
            {
                batch.StockQuantity += dto.StockQuantity;
                _unitOfWork.VaccineBatches.Update(batch);
                await _auditLogService.LogActionAsync(currentUserId, "UpdateVaccineBatch", $"Cộng dồn {dto.StockQuantity} liều vào lô {dto.BatchNumber} cho vắc-xin ID {vaccineId}");
            }
            else
            {
                batch = new VaccineBatch
                {
                    VaccineId = vaccineId,
                    BatchNumber = dto.BatchNumber,
                    ExpirationDate = DateTime.SpecifyKind(dto.ExpirationDate, DateTimeKind.Utc),
                    ImportDate = dto.ImportDate.HasValue ? DateTime.SpecifyKind(dto.ImportDate.Value, DateTimeKind.Utc) : DateTime.UtcNow,
                    StockQuantity = dto.StockQuantity,
                    ImportPrice = dto.ImportPrice,
                    SellingPrice = dto.SellingPrice
                };

                await _unitOfWork.VaccineBatches.AddAsync(batch);
                await _auditLogService.LogActionAsync(currentUserId, "CreateVaccineBatch", $"Nhập lô vắc-xin mới {dto.BatchNumber} cho vắc-xin ID {vaccineId}");
            }
            
            // Note: vaccine.StockQuantity is computed dynamically in Vaccine.cs, so we don't need to manually update it here.
            _unitOfWork.Vaccines.Update(vaccine);

            await _unitOfWork.SaveChangesAsync();

            return new VaccineBatchAdminDto
            {
                Id = batch.Id,
                VaccineId = batch.VaccineId,
                BatchNumber = batch.BatchNumber,
                ExpirationDate = batch.ExpirationDate,
                ImportDate = batch.ImportDate,
                StockQuantity = batch.StockQuantity,
                ImportPrice = batch.ImportPrice,
                SellingPrice = batch.SellingPrice
            };
        }

        public async Task DeleteVaccineBatchAsync(long batchId, string currentUserId)
        {
            var batch = await _unitOfWork.VaccineBatches.GetByIdAsync(batchId) ?? throw new KeyNotFoundException("Không tìm thấy lô vắc-xin.");
            
            var usage = await _unitOfWork.VaccinationRecords.FindAsync(r => r.VaccineBatchId == batchId);
            
            if (usage.Any())
            {
                batch.StockQuantity = 0;
                _unitOfWork.VaccineBatches.Update(batch);
                await _auditLogService.LogActionAsync(currentUserId, "DisposeVaccineBatch", $"Xả kho (set 0) lô vắc-xin ID {batchId} vì đã có dữ liệu tiêm chủng");
            }
            else
            {
                _unitOfWork.VaccineBatches.Remove(batch);
                await _auditLogService.LogActionAsync(currentUserId, "DeleteVaccineBatch", $"Xoá lô vắc-xin ID {batchId}");
            }

            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(batch.VaccineId);
            if (vaccine != null)
            {
                // Dynamic aggregate stock handles the update
                _unitOfWork.Vaccines.Update(vaccine);
            }

            await _unitOfWork.SaveChangesAsync();
        }



        public SlotConfigModel GetSlotConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new SlotConfigModel
                {
                    StartTime = "08:00:00",
                    EndTime = "17:00:00",
                    DurationMinutes = 30,
                    MaxAppointmentsPerSlot = 3
                };
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<SlotConfigModel>(json) ?? new SlotConfigModel();
            }
            catch
            {
                return new SlotConfigModel();
            }
        }

        public async Task UpdateSlotConfigAsync(SlotConfigModel config, string currentUserId)
        {
            SaveConfig(config);
            await _auditLogService.LogActionAsync(currentUserId, "UpdateSlotConfig", $"Cập nhật cấu hình khung giờ làm việc");
        }

        private static void SaveConfig(SlotConfigModel config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
