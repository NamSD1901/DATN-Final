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
    public class MedicineService : IMedicineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMedicineRepository _medicineRepo;
        private readonly IMedicineBatchRepository _batchRepo;
        private readonly IInventoryTransactionRepository _transactionRepo;

        public MedicineService(
            IUnitOfWork unitOfWork,
            IMedicineRepository medicineRepo,
            IMedicineBatchRepository batchRepo,
            IInventoryTransactionRepository transactionRepo)
        {
            _unitOfWork = unitOfWork;
            _medicineRepo = medicineRepo;
            _batchRepo = batchRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync()
        {
            var medicines = await _medicineRepo.GetMedicinesWithStockAsync();
            return medicines.Select(m => new MedicineDto
            {
                Id = m.Id,
                Name = m.Name,
                Unit = m.Unit,
                StockQuantity = m.StockQuantity,
                ImportPrice = m.ImportPrice,
                SellPrice = m.SellPrice
            });
        }

        public async Task<MedicineDetailDto?> GetMedicineDetailsAsync(long id)
        {
            var medicine = await _medicineRepo.GetMedicineWithBatchesAsync(id);
            if (medicine == null) return null;
            
            var category = await _unitOfWork.MedicineCategories.GetByIdAsync(medicine.CategoryId);

            return new MedicineDetailDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                Unit = medicine.Unit,
                StockQuantity = medicine.StockQuantity,
                SellPrice = medicine.SellPrice,
                MedicineCode = medicine.MedicineCode,
                CategoryId = medicine.CategoryId,
                CategoryName = category?.Name ?? "",
                MinStockLevel = medicine.MinStockLevel,
                IsActive = medicine.IsActive,
                Batches = medicine.Batches.Select(b => new MedicineBatchDto
                {
                    Id = b.Id,
                    BatchNumber = b.BatchNumber,
                    MedicineId = b.MedicineId,
                    ManufactureDate = b.ManufactureDate,
                    ExpiryDate = b.ExpiryDate,
                    InitialQuantity = b.InitialQuantity,
                    CurrentQuantity = b.CurrentQuantity
                }).ToList()
            };
        }

        public async Task<MedicineDto?> GetMedicineStockAsync(long id)
        {
            var medicine = await _medicineRepo.GetMedicineWithBatchesAsync(id);
            if (medicine == null) return null;

            return new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                StockQuantity = medicine.StockQuantity,
                ImportPrice = medicine.ImportPrice,
                Unit = medicine.Unit,
                SellPrice = medicine.SellPrice
            };
        }

        public async Task<IEnumerable<MedicineDto>> GetLowStockMedicinesAsync()
        {
            var medicines = await _medicineRepo.GetMedicinesWithStockAsync();
            return medicines.Where(m => m.StockQuantity <= m.MinStockLevel).Select(m => new MedicineDto
            {
                Id = m.Id,
                Name = m.Name,
                Unit = m.Unit,
                StockQuantity = m.StockQuantity,
                ImportPrice = m.ImportPrice,
                SellPrice = m.SellPrice
            });
        }

        public async Task ImportMedicineAsync(ImportMedicineDto dto, Guid userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var medicine = await _unitOfWork.Medicines.GetByIdAsync(dto.MedicineId);
                if (medicine == null) throw new Exception("Thuốc không tồn tại.");

                if (dto.ExpiryDate <= DateTime.UtcNow)
                    throw new Exception("Không thể nhập lô thuốc đã hết hạn.");
                
                if (dto.ManufactureDate > DateTime.UtcNow)
                    throw new Exception("Ngày sản xuất không hợp lệ (lớn hơn ngày hiện tại).");

                var existingBatch = await _batchRepo.GetBatchByNumberAsync(dto.BatchNumber);
                MedicineBatch batch;

                if (existingBatch != null)
                {
                    if (existingBatch.MedicineId != dto.MedicineId)
                        throw new Exception("Số lô đã được sử dụng cho một loại thuốc khác.");
                    
                    batch = existingBatch;
                    batch.CurrentQuantity += dto.Quantity;
                    batch.InitialQuantity += dto.Quantity;
                    _unitOfWork.MedicineBatches.Update(batch);
                }
                else
                {
                    batch = new MedicineBatch
                    {
                        BatchNumber = dto.BatchNumber,
                        MedicineId = dto.MedicineId,
                        ManufactureDate = dto.ManufactureDate,
                        ExpiryDate = dto.ExpiryDate,
                        InitialQuantity = dto.Quantity,
                        CurrentQuantity = dto.Quantity
                    };
                    await _unitOfWork.MedicineBatches.AddAsync(batch);
                }

                await _unitOfWork.SaveChangesAsync();

                var transaction = new InventoryTransaction
                {
                    TransactionDate = DateTime.UtcNow,
                    Type = InventoryTransactionType.GoodsReceipt,
                    MedicineId = dto.MedicineId,
                    BatchId = batch.Id,
                    QuantityChange = dto.Quantity,
                    CreatedByUserId = userId,
                    ReferenceCode = dto.ReferenceCode,
                    Notes = dto.Notes ?? "Nhập kho mới"
                };

                await _unitOfWork.InventoryTransactions.AddAsync(transaction);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ExportMedicineAsync(ExportMedicineDto dto, Guid userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var medicine = await _medicineRepo.GetMedicineWithBatchesAsync(dto.MedicineId);
                if (medicine == null) throw new Exception("Thuốc không tồn tại.");

                if (medicine.StockQuantity < dto.Quantity)
                    throw new Exception("Tồn kho không đủ để xuất.");

                var availableBatches = await _batchRepo.GetAvailableBatchesAsync(dto.MedicineId);
                int remainingToExport = dto.Quantity;

                foreach (var batch in availableBatches)
                {
                    if (remainingToExport <= 0) break;

                    int takeAmount = Math.Min(batch.CurrentQuantity, remainingToExport);
                    batch.CurrentQuantity -= takeAmount;
                    remainingToExport -= takeAmount;

                    _unitOfWork.MedicineBatches.Update(batch);

                    var transaction = new InventoryTransaction
                    {
                        TransactionDate = DateTime.UtcNow,
                        Type = InventoryTransactionType.PrescriptionDispense,
                        MedicineId = dto.MedicineId,
                        BatchId = batch.Id,
                        QuantityChange = -takeAmount,
                        CreatedByUserId = userId,
                        ReferenceCode = dto.ReferenceCode,
                        Notes = dto.Notes ?? "Xuất kho / Sử dụng kê đơn"
                    };
                    await _unitOfWork.InventoryTransactions.AddAsync(transaction);
                }

                if (remainingToExport > 0)
                    throw new Exception("Lỗi hệ thống: Số lượng lô không khớp với tổng tồn kho.");

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task AdjustMedicineStockAsync(AdjustMedicineDto dto, Guid userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var batch = await _batchRepo.GetBatchWithMedicineAsync(dto.BatchId);
                if (batch == null || batch.MedicineId != dto.MedicineId)
                    throw new Exception("Không tìm thấy lô thuốc tương ứng.");

                if (dto.QuantityChange < 0 && batch.CurrentQuantity < Math.Abs(dto.QuantityChange))
                    throw new Exception("Số lượng điều chỉnh giảm vượt quá tồn kho hiện tại của lô.");

                batch.CurrentQuantity += dto.QuantityChange;
                _unitOfWork.MedicineBatches.Update(batch);

                var transaction = new InventoryTransaction
                {
                    TransactionDate = DateTime.UtcNow,
                    Type = InventoryTransactionType.Adjustment,
                    MedicineId = dto.MedicineId,
                    BatchId = batch.Id,
                    QuantityChange = dto.QuantityChange,
                    CreatedByUserId = userId,
                    ReferenceCode = dto.ReferenceCode,
                    Notes = dto.Notes ?? "Điều chỉnh kho"
                };

                await _unitOfWork.InventoryTransactions.AddAsync(transaction);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetMedicineTransactionsAsync(long medicineId)
        {
            var transactions = await _transactionRepo.GetTransactionsByMedicineAsync(medicineId);
            return transactions.Select(t => new InventoryTransactionDto
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                Type = t.Type,
                MedicineId = t.MedicineId,
                MedicineName = t.Medicine?.Name ?? "",
                BatchId = t.BatchId,
                BatchNumber = t.Batch?.BatchNumber ?? "",
                QuantityChange = t.QuantityChange,
                CreatedByUserId = t.CreatedByUserId,
                ReferenceCode = t.ReferenceCode,
                Notes = t.Notes
            });
        }

        public async Task AuditMedicineBatchAsync(AuditMedicineDto dto, Guid userId)
        {
            var batch = await _batchRepo.GetBatchWithMedicineAsync(dto.BatchId);
            if (batch == null || batch.MedicineId != dto.MedicineId)
                throw new Exception("Không tìm thấy lô thuốc tương ứng.");

            int variance = dto.ActualQuantity - batch.CurrentQuantity;
            if (variance == 0) return; // No change

            var adjustDto = new AdjustMedicineDto
            {
                MedicineId = dto.MedicineId,
                BatchId = dto.BatchId,
                QuantityChange = variance,
                ReferenceCode = dto.ReferenceCode,
                Notes = dto.Notes ?? $"Kiểm kê chênh lệch {variance}"
            };

            await AdjustMedicineStockAsync(adjustDto, userId);
        }

        public async Task<IEnumerable<ExpiringMedicineDto>> GetExpiringMedicinesAsync(int daysThreshold)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysThreshold);
            var medicines = await _medicineRepo.GetMedicinesWithStockAsync();
            var expiringList = new List<ExpiringMedicineDto>();

            foreach (var medicine in medicines)
            {
                var batches = await _batchRepo.GetAvailableBatchesAsync(medicine.Id);
                foreach (var batch in batches)
                {
                    if (batch.ExpiryDate <= targetDate)
                    {
                        expiringList.Add(new ExpiringMedicineDto
                        {
                            MedicineId = medicine.Id,
                            MedicineName = medicine.Name,
                            BatchId = batch.Id,
                            BatchNumber = batch.BatchNumber,
                            ExpiryDate = batch.ExpiryDate,
                            CurrentQuantity = batch.CurrentQuantity,
                            DaysUntilExpiry = (batch.ExpiryDate - DateTime.UtcNow).Days
                        });
                    }
                }
            }

            return expiringList.OrderBy(x => x.ExpiryDate);
        }
    }
}
