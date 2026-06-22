namespace MyPetClinic.Domain.Entities
{
    public enum InventoryTransactionType
    {
        GoodsReceipt = 1,          // Nhập kho
        PrescriptionDispense = 2,  // Xuất kho khám bệnh
        Adjustment = 3             // Kiểm kê/Điều chỉnh
    }
}
