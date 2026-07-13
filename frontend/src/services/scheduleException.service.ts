import api from './api';

export interface CreateScheduleExceptionDto {
  doctorId: string;
  type: string; // "TimeOff" or "ShiftSwap"
  startDate: string;
  endDate: string;
  substituteDoctorId?: string;
  reason?: string;
}

export interface ApproveScheduleExceptionDto {
  status: string; // "Approved" or "Rejected"
}

export const scheduleExceptionService = {
  createException: async (dto: CreateScheduleExceptionDto) => {
    const response = await api.post('/ScheduleException', dto);
    return response.data;
  },

  approveException: async (id: number, dto: ApproveScheduleExceptionDto) => {
    const response = await api.post(`/ScheduleException/${id}/Approve`, dto);
    return response.data;
  },

  getPendingExceptions: async () => {
    const response = await api.get('/ScheduleException/Pending');
    return response.data;
  }
};
