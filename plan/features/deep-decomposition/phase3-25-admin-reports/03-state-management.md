# 🗄️ State Management - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **FE-C03 (State Management):** Sử dụng Pinia composition store để quản lý bộ lọc ngày và chia sẻ dữ liệu dashboard giữa các component biểu đồ và bảng số liệu.

---

## 1. Pinia Store: `useRevenueStore`

Sử dụng TypeScript và Composition API (`setup` style) để lưu trữ trạng thái bộ lọc khoảng ngày và dữ liệu dashboard phản hồi từ API.

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';

export interface DailyRevenue {
  date: string;
  revenue: number;
}

export interface TopService {
  name: string;
  count: number;
  value: number;
}

export interface RevenueDashboardData {
  totalRevenue: number;
  growthRate: number;
  totalAppointments: number;
  serviceRevenue: number;
  medicineRevenue: number;
  dailyChart: DailyRevenue[];
  topServices: TopService[];
}

export const useRevenueStore = defineStore('revenueReport', () => {
  // 1. State
  const startDate = ref<string>(
    new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0]
  );
  const endDate = ref<string>(new Date().toISOString().split('T')[0]);
  const dashboardData = ref<RevenueDashboardData | null>(null);
  const isLoading = ref<boolean>(false);
  const errorMessage = ref<string | null>(null);

  // 2. Getters
  const formattedTotalRevenue = computed(() => {
    if (!dashboardData.value) return '0 đ';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(
      dashboardData.value.totalRevenue
    );
  });

  const hasGrowth = computed(() => {
    if (!dashboardData.value) return false;
    return dashboardData.value.growthRate >= 0;
  });

  // 3. Actions
  async function fetchDashboard() {
    isLoading.value = true;
    errorMessage.value = null;
    try {
      const response = await axios.get<RevenueDashboardData>('/api/admin/reports/revenue', {
        params: {
          startDate: startDate.value,
          endDate: endDate.value
        }
      });
      dashboardData.value = response.data;
    } catch (err: any) {
      errorMessage.value = err.response?.data?.message || 'Không thể tải dữ liệu báo cáo.';
    } finally {
      isLoading.value = false;
    }
  }

  function setDateRange(start: string, end: string) {
    startDate.value = start;
    endDate.value = end;
    fetchDashboard();
  }

  return {
    startDate,
    endDate,
    dashboardData,
    isLoading,
    errorMessage,
    formattedTotalRevenue,
    hasGrowth,
    fetchDashboard,
    setDateRange
  };
});
```
