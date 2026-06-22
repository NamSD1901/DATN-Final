import { defineStore } from 'pinia';
import { clinicConfigService } from '../services/clinicConfig.service';

export const useClinicConfigStore = defineStore('clinicConfig', {
  state: () => ({
    weeklyHours: [] as any[],
    holidays: [] as any[],
    isLoading: false,
    error: null as string | null
  }),
  actions: {
    async fetchWeeklyHours() {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await clinicConfigService.getOperatingHours();
        if (response.data.success) {
          this.weeklyHours = response.data.data;
        }
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi lấy cấu hình giờ hoạt động';
      } finally {
        this.isLoading = false;
      }
    },
    async saveWeeklyHours(payload: any) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await clinicConfigService.updateOperatingHours(payload);
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi lưu cấu hình giờ hoạt động';
        throw err;
      } finally {
        this.isLoading = false;
      }
    },
    async fetchHolidays() {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await clinicConfigService.getHolidays();
        if (response.data.success) {
          this.holidays = response.data.data;
        }
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi lấy danh sách ngày nghỉ';
      } finally {
        this.isLoading = false;
      }
    },
    async addHoliday(payload: any) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await clinicConfigService.createHoliday(payload);
        if (response.data.success) {
          await this.fetchHolidays();
        }
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi thêm ngày nghỉ';
        throw err;
      } finally {
        this.isLoading = false;
      }
    },
    async editHoliday(id: number, payload: any) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await clinicConfigService.updateHoliday(id, payload);
        if (response.data.success) {
          await this.fetchHolidays();
        }
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi cập nhật ngày nghỉ';
        throw err;
      } finally {
        this.isLoading = false;
      }
    },
    async removeHoliday(id: number) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await clinicConfigService.deleteHoliday(id);
        if (response.data.success) {
          await this.fetchHolidays();
        }
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi xóa ngày nghỉ';
        throw err;
      } finally {
        this.isLoading = false;
      }
    }
  }
});
