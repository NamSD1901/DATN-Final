# 03. State Management (Pinia Store) - Admin Drug Inventory

Tài liệu thiết kế Vue 3 Pinia Store sử dụng TypeScript cho phân hệ Quản lý Kho thuốc & Vật tư.

---

## 1. Vai trò của Pinia Store trong Phân hệ

Pinia Store `useInventoryStore` chịu trách nhiệm quản lý danh mục thuốc và các lô hàng trong kho. Store hỗ trợ lọc nhanh ở Client-side danh sách thuốc theo trạng thái cảnh báo (`Low Stock`, `Expired`, `Out of Stock`) để quản trị viên có thể xem nhanh các thuốc cần xử lý nhập hàng gấp hoặc xử lý trả lô hàng cận hạn.

---

## 2. Mã nguồn TypeScript hoàn chỉnh cho Pinia Store

Dưới đây là cài đặt chi tiết của file `useInventoryStore.ts` triển khai tại thư mục `frontend/src/stores/useInventoryStore.ts`:

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

// Định nghĩa kiểu dữ liệu cho lô hàng
export interface Batch {
  id: string;
  batchNumber: string;
  currentQuantity: number;
  expiryDate: string;
  batchStatus: 'InStock' | 'Expired' | 'ExpiringSoon';
}

// Định nghĩa kiểu dữ liệu cho biệt dược trong danh mục kho
export interface Medicine {
  id: string;
  name: string;
  activeIngredient: string;
  price: number;
  minStockThreshold: number;
  currentStock: number;
  stockStatus: 'LowStock' | 'InStock' | 'Depleted';
  batches?: Batch[];
}

// Định nghĩa kiểu giao dịch kho
export interface InventoryTransaction {
  id: string;
  medicineName: string;
  batchNumber: string;
  transactionType: 'IMPORT' | 'EXPORT';
  quantity: number;
  actorName: string;
  timestamp: string;
}

interface InventoryState {
  medicines: Medicine[];
  transactions: InventoryTransaction[];
  searchQuery: string;
  alertFilter: 'All' | 'LowStock' | 'Expired' | 'Depleted';
  isLoading: boolean;
  isSubmitting: boolean;
  error: string | null;
}

export const useInventoryStore = defineStore('inventory', {
  state: (): InventoryState => ({
    medicines: [],
    transactions: [],
    searchQuery: '',
    alertFilter: 'All',
    isLoading: false,
    isSubmitting: false,
    error: null
  }),

  getters: {
    // Bộ lọc danh mục thuốc thông minh theo ô tìm kiếm và cảnh báo kho
    filteredMedicines(state): Medicine[] {
      return state.medicines.filter((med) => {
        const query = state.searchQuery.toLowerCase();
        const matchesSearch =
          med.name.toLowerCase().includes(query) ||
          med.activeIngredient.toLowerCase().includes(query);

        let matchesAlert = true;
        if (state.alertFilter === 'LowStock') {
          matchesAlert = med.currentStock <= med.minStockThreshold && med.currentStock > 0;
        } else if (state.alertFilter === 'Depleted') {
          matchesAlert = med.currentStock === 0;
        } else if (state.alertFilter === 'Expired') {
          // Kiểm tra xem có lô nào bị hết hạn không
          matchesAlert = med.batches?.some(b => b.batchStatus === 'Expired') ?? false;
        }

        return matchesSearch && matchesAlert;
      });
    },

    // Thống kê nhanh tổng quan kho để hiển thị lên widgets
    inventorySummary(state) {
      return {
        totalItems: state.medicines.length,
        lowStockItems: state.medicines.filter(m => m.currentStock <= m.minStockThreshold && m.currentStock > 0).length,
        depletedItems: state.medicines.filter(m => m.currentStock === 0).length,
        expiringBatchesCount: state.medicines.reduce((sum, m) => {
          return sum + (m.batches?.filter(b => b.batchStatus === 'ExpiringSoon').length || 0);
        }, 0)
      };
    }
  },

  actions: {
    // 1. Tải danh mục thuốc kèm số lượng tồn và lô hàng
    async fetchMedicines() {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await axios.get<Medicine[]>('/api/admin/inventory/medicines');
        this.medicines = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh mục kho thuốc.';
        console.error('Error fetching inventory:', err);
      } finally {
        this.isLoading = false;
      }
    },

    // 2. Nhập một lô thuốc mới vào kho
    async importBatch(data: { medicineId: string; batchNumber: string; quantity: number; expiryDate: string }) {
      this.isSubmitting = true;
      this.error = null;
      try {
        const response = await axios.post<Batch>('/api/admin/inventory/batches', data);
        
        // Cập nhật lại số lượng tồn kho của biệt dược tương ứng tại Client-side (Tránh reload toàn trang)
        const medIndex = this.medicines.findIndex(m => m.id === data.medicineId);
        if (medIndex !== -1) {
          const med = this.medicines[medIndex];
          med.currentStock += data.quantity;
          
          if (!med.batches) med.batches = [];
          med.batches.push(response.data);
          
          // Re-evaluate stock status
          med.stockStatus = med.currentStock > med.minStockThreshold ? 'InStock' : 'LowStock';
        }
        
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi xảy ra khi thực hiện nhập kho.';
        throw err;
      } finally {
        this.isSubmitting = false;
      }
    },

    // 3. Tải lịch sử biến động giao dịch nhập/xuất kho
    async fetchTransactions() {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await axios.get<InventoryTransaction[]>('/api/admin/inventory/transactions');
        this.transactions = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải lịch sử giao dịch kho.';
        console.error(err);
      } finally {
        this.isLoading = false;
      }
    }
  }
});
```
