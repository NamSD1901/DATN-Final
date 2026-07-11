import { defineStore } from 'pinia';
import { doctorScheduleService, type DoctorSchedule, type DoctorScheduleCreate, type DoctorScheduleUpdate } from '../services/doctorSchedule.service';
import Swal from 'sweetalert2';

export const useDoctorScheduleStore = defineStore('doctorSchedule', {
  state: () => ({
    schedules: [] as DoctorSchedule[],
    isLoading: false,
    error: null as string | null
  }),
  actions: {
    async fetchSchedules(startDate?: string, endDate?: string, doctorId?: string) {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await doctorScheduleService.getSchedules(startDate, endDate, doctorId);
        this.schedules = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi khi tải danh sách ca trực';
        Swal.fire('Lỗi', this.error, 'error');
      } finally {
        this.isLoading = false;
      }
    },
    async createSchedule(data: DoctorScheduleCreate) {
      this.isLoading = true;
      this.error = null;
      try {
        await doctorScheduleService.createSchedule(data);
        Swal.fire('Thành công', 'Thêm ca trực thành công', 'success');
        return true;
      } catch (err: any) {
        const errorData = err.response?.data;
        this.error = errorData?.message 
          || (errorData?.errors ? Object.values(errorData.errors).flat().join(', ') : null) 
          || err.message
          || 'Lỗi khi thêm ca trực';
        Swal.fire('Lỗi', this.error, 'error');
        return false;
      } finally {
        this.isLoading = false;
      }
    },
    async updateSchedule(id: number, data: DoctorScheduleUpdate) {
      this.isLoading = true;
      this.error = null;
      try {
        await doctorScheduleService.updateSchedule(id, data);
        Swal.fire('Thành công', 'Cập nhật ca trực thành công', 'success');
        return true;
      } catch (err: any) {
        const errorData = err.response?.data;
        this.error = errorData?.message 
          || (errorData?.errors ? Object.values(errorData.errors).flat().join(', ') : null) 
          || err.message
          || 'Lỗi khi cập nhật ca trực';
        Swal.fire('Lỗi', this.error, 'error');
        return false;
      } finally {
        this.isLoading = false;
      }
    },
    async deleteSchedule(id: number) {
      this.isLoading = true;
      this.error = null;
      try {
        await doctorScheduleService.deleteSchedule(id);
        Swal.fire('Thành công', 'Xóa ca trực thành công', 'success');
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể xóa ca trực';
        Swal.fire('Lỗi', this.error, 'error');
        return false;
      } finally {
        this.isLoading = false;
      }
    }
  }
});
