import api from './api';

export interface DoctorSchedule {
  id: number;
  doctorId: string;
  doctorName: string;
  workDate: string;
  startTime: string;
  endTime: string;
  maxAppointments: number | null;
  isAvailable: boolean;
  recurringGroupId?: string | null;
  notes?: string | null;
}

export interface DoctorScheduleCreate {
  doctorId: string;
  workDate: string;
  startTime: string;
  endTime: string;
  maxAppointments?: number | null;
  recurringGroupId?: string | null;
  notes?: string | null;
}

export interface DoctorScheduleUpdate {
  startTime: string;
  endTime: string;
  maxAppointments?: number | null;
  isAvailable: boolean;
  recurringGroupId?: string | null;
  notes?: string | null;
}

export const doctorScheduleService = {
  getSchedules(startDate?: string, endDate?: string, doctorId?: string) {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (doctorId) params.append('doctorId', doctorId);
    return api.get<DoctorSchedule[]>(`/doctor-schedules?${params.toString()}`);
  },

  getScheduleById(id: number) {
    return api.get<DoctorSchedule>(`/doctor-schedules/${id}`);
  },

  createSchedule(data: DoctorScheduleCreate) {
    return api.post<{ success: boolean; id: number }>('/doctor-schedules', data);
  },

  updateSchedule(id: number, data: DoctorScheduleUpdate) {
    return api.put<{ success: boolean }>(`/doctor-schedules/${id}`, data);
  },

  deleteSchedule(id: number) {
    return api.delete<{ success: boolean }>(`/doctor-schedules/${id}`);
  }
};
