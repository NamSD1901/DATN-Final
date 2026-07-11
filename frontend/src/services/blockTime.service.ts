import api from './api';

export type BlockType = number;

export interface BlockTime {
  id: string;
  doctorId: string;
  doctorName: string;
  startTime: string;
  endTime: string;
  blockType: BlockType;
  reason?: string | null;
  backgroundColor?: string | null;
}

export interface BlockTimeCreate {
  doctorId: string;
  startTime: string;
  endTime: string;
  blockType: BlockType;
  reason?: string | null;
  backgroundColor?: string | null;
}

export interface BlockTimeUpdate {
  startTime: string;
  endTime: string;
  blockType: BlockType;
  reason?: string | null;
  backgroundColor?: string | null;
}

export const blockTimeService = {
  getBlockTimes(startDate?: string, endDate?: string, doctorId?: string) {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (doctorId) params.append('doctorId', doctorId);
    return api.get<BlockTime[]>(`/block-times?${params.toString()}`);
  },

  getBlockTimeById(id: string) {
    return api.get<BlockTime>(`/block-times/${id}`);
  },

  createBlockTime(data: BlockTimeCreate) {
    return api.post<{ success: boolean; id: string }>('/block-times', data);
  },

  updateBlockTime(id: string, data: BlockTimeUpdate) {
    return api.put<{ success: boolean }>(`/block-times/${id}`, data);
  },

  deleteBlockTime(id: string) {
    return api.delete<{ success: boolean }>(`/block-times/${id}`);
  }
};
