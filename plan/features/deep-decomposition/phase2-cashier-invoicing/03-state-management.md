# 03. State Management (Pinia Store) - Cashier & Invoicing

Tài liệu thiết kế Vue 3 Pinia Store sử dụng TypeScript cho phân hệ Quản lý Hóa đơn & Thu ngân phòng khám.

---

## 1. Vai trò của Pinia Store trong Phân hệ

Pinia Store `useInvoiceStore` đóng vai trò quản lý tập trung trạng thái các hóa đơn chờ thanh toán tại quầy, phục vụ tìm kiếm nhanh theo số hóa đơn, số điện thoại chủ thú cưng, hoặc tên thú cưng mà không cần gọi API liên tục. Đồng thời, store điều phối việc kích hoạt popup mã QR động, lưu vết hóa đơn đang được in nhiệt, và cập nhật trạng thái bất đồng bộ giữa giao diện Lễ tân và DB Backend.

---

## 2. Mã nguồn TypeScript hoàn chỉnh cho Pinia Store

Dưới đây là mã nguồn chi tiết của file `useInvoiceStore.ts` triển khai trong thư mục `frontend/src/stores/useInvoiceStore.ts`:

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

// Định nghĩa kiểu dữ liệu chi tiết của dòng hóa đơn
export interface InvoiceItem {
  id: string;
  itemName: string;
  itemType: 'Service' | 'Medicine' | 'Vaccine';
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

// Định nghĩa kiểu dữ liệu chi tiết của Hóa đơn
export interface Invoice {
  id: string;
  appointmentId: string;
  invoiceNumber: string;
  totalAmount: number;
  paymentMethod: 'Cash' | 'BankTransfer' | null;
  status: 'Pending' | 'Paid' | 'Cancelled';
  createdAt: string;
  paidAt: string | null;
  customerName: string;
  petName: string;
  items?: InvoiceItem[];
}

// Định nghĩa kiểu dữ liệu phản hồi VietQR
export interface VietQrResponse {
  qrCodeUrl: string;
  accountNo: string;
  accountName: string;
  amount: number;
  description: string;
}

interface InvoiceState {
  invoices: Invoice[];
  currentInvoice: Invoice | null;
  searchQuery: string;
  statusFilter: 'All' | 'Pending' | 'Paid' | 'Cancelled';
  isLoading: boolean;
  isSubmitting: boolean;
  qrPayload: VietQrResponse | null;
  error: string | null;
}

export const useInvoiceStore = defineStore('invoice', {
  state: (): InvoiceState => ({
    invoices: [],
    currentInvoice: null,
    searchQuery: '',
    statusFilter: 'Pending', // Mặc định hiển thị hóa đơn chờ
    isLoading: false,
    isSubmitting: false,
    qrPayload: null,
    error: null
  }),

  getters: {
    // Bộ lọc danh sách hóa đơn theo từ khóa tìm kiếm và trạng thái lọc
    filteredInvoices(state): Invoice[] {
      return state.invoices.filter((inv) => {
        const matchesSearch =
          inv.invoiceNumber.toLowerCase().includes(state.searchQuery.toLowerCase()) ||
          inv.customerName.toLowerCase().includes(state.searchQuery.toLowerCase()) ||
          inv.petName.toLowerCase().includes(state.searchQuery.toLowerCase());
        
        const matchesStatus =
          state.statusFilter === 'All' || inv.status === state.statusFilter;

        return matchesSearch && matchesStatus;
      });
    },

    // Tổng tiền của tất cả hóa đơn đang chờ (để làm báo cáo nhanh tại quầy)
    pendingTotalValue(): number {
      return this.filteredInvoices
        .filter((inv) => inv.status === 'Pending')
        .reduce((sum, inv) => sum + inv.totalAmount, 0);
    }
  },

  actions: {
    // 1. Tải danh sách hóa đơn chờ từ Backend
    async fetchPendingInvoices() {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await axios.get<Invoice[]>('/api/receptionist/invoices/pending');
        this.invoices = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh sách hóa đơn chờ.';
        console.error('Error fetching pending invoices:', err);
      } finally {
        this.isLoading = false;
      }
    },

    // 2. Tạo hóa đơn mới từ một Lịch hẹn khám đã hoàn thành
    async createInvoice(appointmentId: string) {
      this.isSubmitting = true;
      this.error = null;
      try {
        const response = await axios.post<Invoice>('/api/receptionist/invoices', { appointmentId });
        // Thêm vào danh sách hiện tại
        this.invoices.unshift(response.data);
        this.currentInvoice = response.data;
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tạo hóa đơn từ ca khám này.';
        throw err;
      } finally {
        this.isSubmitting = false;
      }
    },

    // 3. Tải thông tin chi tiết một hóa đơn (bao gồm InvoiceItems)
    async fetchInvoiceDetails(invoiceId: string) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await axios.get<Invoice>(`/api/receptionist/invoices/${invoiceId}`);
        this.currentInvoice = response.data;
        // Cập nhật lại đối tượng trong mảng danh sách nếu có
        const index = this.invoices.findIndex((inv) => inv.id === invoiceId);
        if (index !== -1) {
          this.invoices[index] = response.data;
        }
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải chi tiết hóa đơn.';
        console.error(err);
      } finally {
        this.isLoading = false;
      }
    },

    // 4. Sinh mã QR động cho hóa đơn chuyển khoản
    async fetchQrCode(invoiceId: string) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await axios.get<VietQrResponse>(`/api/receptionist/invoices/${invoiceId}/qr`);
        this.qrPayload = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tạo mã VietQR cho hóa đơn.';
        console.error(err);
      } finally {
        this.isLoading = false;
      }
    },

    // 5. Xác nhận thanh toán hóa đơn thành công
    async confirmPayment(invoiceId: string, paymentMethod: 'Cash' | 'BankTransfer') {
      this.isSubmitting = true;
      this.error = null;
      try {
        const response = await axios.put<Invoice>(`/api/receptionist/invoices/${invoiceId}/pay`, {
          paymentMethod
        });
        
        // Cập nhật trong store
        this.currentInvoice = response.data;
        const index = this.invoices.findIndex((inv) => inv.id === invoiceId);
        if (index !== -1) {
          this.invoices[index] = response.data;
        }
        
        // Nếu đã thanh toán xong, xóa khỏi danh sách chờ nếu đang lọc Pending
        if (this.statusFilter === 'Pending') {
          this.invoices = this.invoices.filter((inv) => inv.id !== invoiceId);
        }
        
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Thanh toán hóa đơn thất bại.';
        throw err;
      } finally {
        this.isSubmitting = false;
      }
    },

    // Reset trạng thái hóa đơn hiện tại
    clearCurrentInvoice() {
      this.currentInvoice = null;
      this.qrPayload = null;
    }
  }
});
```
