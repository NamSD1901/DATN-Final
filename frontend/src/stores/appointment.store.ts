import { defineStore } from 'pinia';
import { appointmentService, type EligibleDoctor, type ChangeDoctorRequest } from '../services/appointment.service';
import Swal from 'sweetalert2';

export const useAppointmentStore = defineStore('appointment', {
  state: () => ({
    eligibleDoctors: [] as EligibleDoctor[],
    isLoadingDoctors: false,
    isChangingDoctor: false,
    error: null as string | null
  }),
  actions: {
    async fetchEligibleDoctors(appointmentId: number) {
      this.isLoadingDoctors = true;
      this.error = null;
      try {
        const response = await appointmentService.getEligibleDoctors(appointmentId);
        this.eligibleDoctors = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi khi tải danh sách bác sĩ phù hợp';
        Swal.fire('Lỗi', this.error || 'Lỗi không xác định', 'error');
      } finally {
        this.isLoadingDoctors = false;
      }
    },
    async changeDoctor(appointmentId: number, data: ChangeDoctorRequest) {
      this.isChangingDoctor = true;
      this.error = null;
      try {
        await appointmentService.changeDoctor(appointmentId, data);
        Swal.fire('Thành công', 'Đổi bác sĩ thành công', 'success');
        return true;
      } catch (err: any) {
        const errorData = err.response?.data;
        this.error = errorData?.message || 'Lỗi khi đổi bác sĩ';
        // Nếu lỗi do bị trùng lịch hoặc nghỉ phép, ta throw ra để component bắt lại và hiện thông báo hỏi Confirm (Force = true)
        if (this.error?.includes('đang trong thời gian nghỉ phép') || this.error?.includes('đã có lịch hẹn')) {
           throw err;
        } else {
           Swal.fire('Lỗi', this.error || 'Lỗi không xác định', 'error');
        }
        return false;
      } finally {
        this.isChangingDoctor = false;
      }
    }
  }
});
