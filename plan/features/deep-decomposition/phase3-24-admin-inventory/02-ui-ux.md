# 🎨 UI/UX Design Specification - Admin Drug Inventory

Tài liệu thiết kế giao diện người dùng, sơ đồ bố cục ASCII Mockup, bảng chỉ màu HSL và hiệu ứng trực quan cảnh báo tồn kho.

---

## 1. Bản vẽ Bố cục giao diện (ASCII Art Mockups)

### Màn hình Quản trị Kho thuốc (Admin Inventory Management Workspace)
Thiết kế mờ kính tối sang trọng, hiển thị bộ lọc thông minh các cảnh báo và bảng danh mục thuốc kèm số lô.

```text
+-------------------------------------------------------------------------------------------------------------------+
|  [Logo] MYPETCLINIC - CỔNG QUẢN TRỊ VIÊN (INVENTORY DASHBOARD)                        [NV: Khánh] [Đăng xuất]     |
+-------------------------------------------------------------------------------------------------------------------+
|  [Tìm kiếm tên thuốc / hoạt chất...]   ( Cảnh báo: [Tất cả]  [(*) Hết hàng (2)]  [Cận hạn (3)] )  [+ NHẬP THUỐC MỚI] |
+-------------------------------------------------------------------------------------------------------------------+
|  DANH MỤC THUỐC TRONG KHO (12 loại thuốc)                                                                         |
|  +-------------------------------------------------------------------------------------------------------------+  |
|  | TÊN BIỆT DƯỢC   | HOẠT CHẤT       | TỒN KHO   | ĐỊNH MỨC   | TRẠNG THÁI     | LÔ HÀNG GẦN NHẤT  | HÀNH ĐỘNG         |  |
|  +-------------------------------------------------------------------------------------------------------------+  |
|  | Amoxicillin     | Amoxicillin     | 450 viên  | 100 viên   | [ Còn hàng ]   | Lô #2606 (12/2026)| [Nhập kho] [Sửa]  |  |
|  | Dexafort        | Dexamethasone   | 8 lọ      | 15 lọ      | [(!) Low Stock]| Lô #2605 (09/2026)| [Nhập kho] [Sửa]  |  |
|  | Rabisin (Vac)   | Dại dại bất hoạt| 4 liều    | 10 liều    | [(!) Low Stock]| Lô #2601 (07/2026)| [Nhập kho] [Sửa]  |  |
|  | NexGard         | Afoxolaner      | 0 hộp     | 5 hộp      | [(X) Depleted] | Không có          | [Nhập kho] [Sửa]  |  |
|  | Hapacol         | Paracetamol     | 120 viên  | 50 viên    | [(!) Cận Hạn]  | Lô #2507 (07/2026)| [Nhập kho] [Sửa]  |  |
|  +-------------------------------------------------------------------------------------------------------------+  |
|                                                                                                                   |
+-------------------------------------------------------------------------------------------------------------------+
```

### Popup Nhập lô hàng mới (Import Batch Modal)
```text
+---------------------------------------------------------+
|                  NHẬP LÔ THUỐC MỚI VÀO KHO              |
+---------------------------------------------------------+
|                                                         |
|  Chọn thuốc:                                            |
|  +---------------------------------------------------+  |
|  | Dexafort (Dexamethasone)                          |  |
|  +---------------------------------------------------+  |
|                                                         |
|  Số lô sản xuất (Batch Number):                         |
|  +---------------------------------------------------+  |
|  | LOT-202606-02                                     |  |
|  +---------------------------------------------------+  |
|                                                         |
|  Số lượng nhập:                     Hạn sử dụng (Expiry):|
|  +---------------+                  +-----------------+  |
|  | 50            |                  | 31/12/2027      |  |
|  +---------------+                  +-----------------+  |
|                                                         |
|  * Giao dịch sẽ được ghi nhận vào nhật ký kiểm toán.    |
|  +---------------------------------------------------+  |
|  |       [ HỦY BỎ ]             [ XÁC NHẬN NHẬP KHO ]    |
|  +---------------------------------------------------+  |
+---------------------------------------------------------+
```

---

## 2. Hệ thống CSS Design Tokens (HSL Color Theme)

Màu sắc HSL tối ưu cho cảnh báo kho y tế:

```css
:root {
  /* Biến màu trạng thái kho */
  --stock-instock: hsl(145, 65%, 45%);       /* Xanh lá - An toàn */
  --stock-instock-bg: hsla(145, 65%, 45%, 0.12);
  
  --stock-low: hsl(35, 95%, 55%);           /* Cam - Sắp hết hàng */
  --stock-low-bg: hsla(35, 95%, 55%, 0.12);
  
  --stock-depleted: hsl(355, 75%, 50%);      /* Đỏ - Hết hàng */
  --stock-depleted-bg: hsla(355, 75%, 50%, 0.12);
  
  --stock-expired: hsl(0, 0%, 50%);          /* Xám - Lô thuốc hết hạn */
  --stock-expired-bg: hsla(0, 0%, 50%, 0.12);

  --backdrop-blur: blur(12px);
  --glass-bg: rgba(15, 23, 42, 0.65);        /* Nền tối thẳm */
  --glass-border: rgba(255, 255, 255, 0.08);
}
```

---

## 3. Hiệu ứng Cảnh báo Trực quan nhấp nháy (Warning Animations)

Khi thuốc rơi vào trạng thái `Low Stock` hoặc `Depleted`, hệ thống sử dụng hiệu ứng nhấp nháy phát sáng nhẹ để thu hút sự chú ý của thủ kho:

```css
@keyframes pulse-red {
  0% {
    box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.4);
  }
  70% {
    box-shadow: 0 0 0 8px rgba(239, 68, 68, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(239, 68, 68, 0);
  }
}

.badge-depleted {
  background-color: var(--stock-depleted-bg);
  color: var(--stock-depleted);
  border: 1px solid var(--stock-depleted);
  animation: pulse-red 2s infinite;
}

@keyframes pulse-orange {
  0% {
    box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.4);
  }
  70% {
    box-shadow: 0 0 0 8px rgba(245, 158, 11, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(245, 158, 11, 0);
  }
}

.badge-low-stock {
  background-color: var(--stock-low-bg);
  color: var(--stock-low);
  border: 1px solid var(--stock-low);
  animation: pulse-orange 2s infinite;
}
```

---

## 4. Logic Client-side Validation cho Nhập kho

```typescript
// InventoryValidation.ts
export interface BatchImportData {
  medicineId: string;
  batchNumber: string;
  quantity: number;
  expiryDate: string;
}

export function validateImportBatch(data: BatchImportData): { isValid: boolean; error?: string } {
  if (!data.medicineId) {
    return { isValid: false, error: 'Vui lòng chọn loại thuốc cần nhập kho.' };
  }

  if (!data.batchNumber || data.batchNumber.trim().length === 0) {
    return { isValid: false, error: 'Số lô sản xuất bắt buộc phải nhập.' };
  }

  if (!data.quantity || data.quantity <= 0) {
    return { isValid: false, error: 'Số lượng nhập kho phải lớn hơn 0.' };
  }

  if (!data.expiryDate) {
    return { isValid: false, error: 'Hạn sử dụng bắt buộc phải nhập.' };
  }

  const expiry = new Date(data.expiryDate);
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  if (expiry <= today) {
    return { isValid: false, error: 'Hạn sử dụng của lô thuốc mới nhập phải lớn hơn ngày hôm nay.' };
  }

  return { isValid: true };
}
```
