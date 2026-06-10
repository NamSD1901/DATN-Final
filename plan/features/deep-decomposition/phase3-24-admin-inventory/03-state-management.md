# 🗃️ State Management - Admin Drug Inventory

## 🔗 Skills Liên Quan
- **FE-C03 (Pinia):** Store `useInventoryStore` quản lý danh sách thuốc, bộ lọc tìm kiếm và các cảnh báo tồn kho.

---

## 1. Pinia Store: `useInventoryStore`

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';

export interface Medicine {
  id: string;
  name: string;
  unit: string;
  stockQuantity: number;
  minStockLimit: number;
  importPrice: number;
  price: number;
  expiryDate?: string;
}

export const useInventoryStore = defineStore('inventory', () => {
  const medicines = ref<Medicine[]>([]);
  const loading = ref(false);
  const searchQuery = ref('');

  const filteredMedicines = computed(() =>
    medicines.value.filter(m =>
      m.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  );

  const lowStockCount = computed(() =>
    medicines.value.filter(m => m.stockQuantity < m.minStockLimit).length
  );

  async fn fetchMedicines() {
    loading.value = true;
    try {
      const response = await axios.get('/api/admin/medicines');
      medicines.value = response.data;
    } finally {
      loading.value = false;
    }
  }

  async fn restock(id: string, quantity: number, notes: string) {
    await axios.post(`/api/admin/medicines/${id}/restock`, { adjustQuantity: quantity, notes });
    const m = medicines.value.find(x => x.id === id);
    if (m) m.stockQuantity += quantity;
  }

  return {
    medicines,
    loading,
    searchQuery,
    filteredMedicines,
    lowStockCount,
    fetchMedicines,
    restock
  };
});
```
