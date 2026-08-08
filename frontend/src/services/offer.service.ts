import api from './api';

export interface Offer {
    id: string;
    code: string;
    name: string;
    description: string;
    discountType: 'PERCENTAGE' | 'FIXED_AMOUNT';
    discountValue: number;
    maxDiscount: number | null;
    minOrderValue: number;
    totalQuantity: number | null;
    usedQuantity: number;
    usageLimitPerUser: number;
    startDate: string;
    endDate: string;
    status: 'ACTIVE' | 'LOCKED' | 'EXPIRED';
    isPublic: boolean;
    appliedServiceIds: number[];
}

export interface CreateOfferDto {
    code: string;
    name: string;
    description: string;
    discountType: 'PERCENTAGE' | 'FIXED_AMOUNT';
    discountValue: number;
    maxDiscount: number | null;
    minOrderValue: number;
    totalQuantity: number | null;
    usageLimitPerUser: number;
    startDate: string;
    endDate: string;
    isPublic: boolean;
    appliedServiceIds: number[];
}

export const offerService = {
    // ADMIN ENDPOINTS
    getOffers(page = 1, limit = 10, status?: string, search?: string) {
        let url = `/offers?page=${page}&limit=${limit}`;
        if (status) url += `&status=${status}`;
        if (search) url += `&search=${search}`;
        return api.get(url);
    },

    getOfferById(id: string) {
        return api.get(`/offers/${id}`);
    },

    createOffer(data: CreateOfferDto) {
        return api.post('/offers', data);
    },

    updateOffer(id: string, data: Partial<CreateOfferDto>) {
        return api.put(`/offers/${id}`, data);
    },

    lockOffer(id: string) {
        return api.patch(`/offers/${id}/lock`);
    },

    // CUSTOMER ENDPOINTS
    getPublicOffers(page = 1, limit = 10, customerId: string | null = null) {
        const url = customerId 
            ? `/offers/public?page=${page}&limit=${limit}&customerId=${customerId}`
            : `/offers/public?page=${page}&limit=${limit}`;
        return api.get(url);
    },

    validateOffer(data: { code: string, orderAmount: number, serviceIds: number[] }) {
        return api.post('/offers/validate', data);
    }
};
