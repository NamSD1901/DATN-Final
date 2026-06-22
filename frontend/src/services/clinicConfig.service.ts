import api from './api';

export const clinicConfigService = {
  getOperatingHours: () => api.get('/OperatingHours'),
  updateOperatingHours: (data: any) => api.put('/OperatingHours', data),
  getHolidays: () => api.get('/OperatingHours/holidays'),
  createHoliday: (data: any) => api.post('/OperatingHours/holidays', data),
  updateHoliday: (id: number, data: any) => api.put(`/OperatingHours/holidays/${id}`, data),
  deleteHoliday: (id: number) => api.delete(`/OperatingHours/holidays/${id}`)
};
