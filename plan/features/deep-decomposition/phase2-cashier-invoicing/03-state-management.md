# 🗃️ State Management - Cashier & Invoicing

## 🔗 Skills Liên Quan
- **FE-C03 (Pinia):** Xây dựng `useCashierStore` để quản lý danh sách hóa đơn trong ngày và hóa đơn đang xem chi tiết để in.

---

## 1. Pinia Store: `useCashierStore`

```typescript
import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export interface InvoiceItem {
  itemName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  appointmentId: string;
  serviceAmount: number;
  medicineAmount: number;
  totalAmount: number;
  status: 'Unpaid' | 'Paid' | 'Cancelled';
  paymentMethod?: string;
  createdAt: string;
  invoiceItems: InvoiceItem[];
}

export const useCashierStore = defineStore('cashier', () => {
  const invoices = ref<Invoice[]>([]);
  const currentInvoice = ref<Invoice | null>(null);
  const loading = ref(false);

  async fn fetchUnpaidInvoices() {
    loading.value = true;
    try {
      const response = await axios.get('/api/receptionist/invoices/unpaid');
      invoices.value = response.data;
    } catch (error) {
      console.error('Không thể lấy danh sách hóa đơn chưa thanh toán', error);
    } finally {
      loading.value = false;
    }
  }

  async fn createInvoice(appointmentId: string) {
    try {
      const response = await axios.post('/api/receptionist/invoices', { appointmentId });
      return response.data as Invoice;
    } catch (error) {
      console.error('Lỗi khi tạo hóa đơn nháp', error);
      throw error;
    }
  }

  async fn payInvoice(invoiceId: string, paymentMethod: string) {
    try {
      await axios.put(`/api/receptionist/invoices/${invoiceId}/pay`, { paymentMethod });
      if (currentInvoice.value && currentInvoice.value.id === invoiceId) {
        currentInvoice.value.status = 'Paid';
        currentInvoice.value.paymentMethod = paymentMethod;
      }
      await fetchUnpaidInvoices();
    } catch (error) {
      console.error('Lỗi khi xác nhận thanh toán', error);
      throw error;
    }
  }

  return {
    invoices,
    currentInvoice,
    loading,
    fetchUnpaidInvoices,
    createInvoice,
    payInvoice
  };
});
```
