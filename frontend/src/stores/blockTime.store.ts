import { defineStore } from 'pinia';
import { blockTimeService, type BlockTime, type BlockTimeCreate, type BlockTimeUpdate } from '../services/blockTime.service';
import Swal from 'sweetalert2'; // Assuming notification store exists

export const useBlockTimeStore = defineStore('blockTime', {
  state: () => ({
    blockTimes: [] as BlockTime[],
    isLoading: false,
    error: null as string | null
  }),
  actions: {
    async fetchBlockTimes(startDate?: string, endDate?: string, doctorId?: string) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await blockTimeService.getBlockTimes(startDate, endDate, doctorId);
        this.blockTimes = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi khi tải danh sách block time';
        Swal.fire('Lỗi', this.error || 'Lỗi không xác định', 'error');
      } finally {
        this.isLoading = false;
      }
    },
    async createBlockTime(data: BlockTimeCreate) {
      this.isLoading = true;
      this.error = null;
      try {
        await blockTimeService.createBlockTime(data);
        Swal.fire('Thành công', 'Thêm thời gian block lịch thành công', 'success');
        return true;
      } catch (err: any) {
        const errorData = err.response?.data;
        this.error = errorData?.message 
          || (errorData?.errors ? Object.values(errorData.errors).flat().join(', ') : null) 
          || err.message
          || 'Lỗi khi thêm block time';
        Swal.fire('Lỗi', this.error || 'Lỗi không xác định', 'error');
        return false;
      } finally {
        this.isLoading = false;
      }
    },
    async updateBlockTime(id: string, data: BlockTimeUpdate) {
      this.isLoading = true;
      this.error = null;
      try {
        await blockTimeService.updateBlockTime(id, data);
        Swal.fire('Thành công', 'Cập nhật thời gian block lịch thành công', 'success');
        return true;
      } catch (err: any) {
        const errorData = err.response?.data;
        this.error = errorData?.message 
          || (errorData?.errors ? Object.values(errorData.errors).flat().join(', ') : null) 
          || err.message
          || 'Lỗi khi cập nhật block time';
        Swal.fire('Lỗi', this.error || 'Lỗi không xác định', 'error');
        return false;
      } finally {
        this.isLoading = false;
      }
    },
    async deleteBlockTime(id: string) {
      this.isLoading = true;
      this.error = null;
      try {
        await blockTimeService.deleteBlockTime(id);
        Swal.fire('Thành công', 'Xóa thời gian block lịch thành công', 'success');
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể xóa block time';
        Swal.fire('Lỗi', this.error || 'Lỗi không xác định', 'error');
        return false;
      } finally {
        this.isLoading = false;
      }
    }
  }
});
