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
  createProfile: async (dto: CreateScheduleProfileDto) => {
    const response = await api.post('/ScheduleProfile', dto);
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
