import api from './api';

export interface ScheduleProfileShift {
  dayOfWeek: number;
  startTime: string; // HH:mm:ss format expected by backend for TimeSpan
  endTime: string;
  isDayOff: boolean;
}

export interface CreateScheduleProfileDto {
  name: string;
  description?: string;
  shifts: ScheduleProfileShift[];
}

export interface AssignProfileDto {
  profileId: number;
  doctorIds: string[];
  effectiveDate: string; // ISO format string
  endDate?: string;
}

export const scheduleProfileService = {
  getProfiles: async () => {
    const response = await api.get('/ScheduleProfile');
    return response.data;
  },

  getProfile: async (id: number) => {
    const response = await api.get(`/ScheduleProfile/${id}`);
    return response.data;
  },

  createProfile: async (dto: CreateScheduleProfileDto) => {
    const response = await api.post('/ScheduleProfile', dto);
    return response.data;
  },

  updateProfile: async (id: number, dto: CreateScheduleProfileDto) => {
    const response = await api.put(`/ScheduleProfile/${id}`, dto);
    return response.data;
  },

  deleteProfile: async (id: number) => {
    const response = await api.delete(`/ScheduleProfile/${id}`);
    return response.data;
  },

  assignProfile: async (dto: AssignProfileDto) => {
    const response = await api.post('/ScheduleProfile/Assign', dto);
    return response.data;
  },

  generateSchedule: async (doctorId: string, daysToGenerate: number = 30) => {
    const response = await api.post(`/ScheduleProfile/Generate?doctorId=${doctorId}&daysToGenerate=${daysToGenerate}`);
    return response.data;
  }
};
