import api from './api';

export interface EligibleDoctor {
  doctorId: string;
  fullName: string;
  totalScore: number;
  tags: string[];
}

export interface ChangeDoctorRequest {
  newDoctorId: string;
  reason: string;
  force: boolean;
}

export const appointmentService = {
  getEligibleDoctors(appointmentId: number) {
    return api.get<EligibleDoctor[]>(`/appointment/${appointmentId}/eligible-doctors`);
  },
  
  changeDoctor(appointmentId: number, data: ChangeDoctorRequest) {
    return api.put(`/appointment/${appointmentId}/change-doctor`, data);
  }
};
