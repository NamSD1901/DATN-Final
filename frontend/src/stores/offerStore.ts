import { defineStore } from 'pinia';
import { offerService, type Offer, type CreateOfferDto } from '../services/offer.service';

export const useOfferStore = defineStore('offer', {
    state: () => ({
        offers: [] as Offer[],
        publicOffers: [] as Offer[],
        totalCount: 0,
        totalPages: 0,
        currentPage: 1,
        pageSize: 10,
        loading: false,
        error: null as string | null
    }),
    actions: {
        async fetchOffers(page = 1, limit = 10, status?: string, search?: string) {
            this.loading = true;
            this.error = null;
            try {
                const response = await offerService.getOffers(page, limit, status, search);
                if (response.data.success) {
                    this.offers = response.data.data.items;
                    this.totalCount = response.data.data.totalCount;
                    this.totalPages = response.data.data.totalPages;
                    this.currentPage = response.data.data.pageIndex || page;
                }
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Failed to fetch offers';
            } finally {
                this.loading = false;
            }
        },

        async fetchPublicOffers(page = 1, limit = 10, customerId = null as string | null) {
            this.loading = true;
            this.error = null;
            try {
                const response = await offerService.getPublicOffers(page, limit, customerId);
                if (response.data.success) {
                    this.publicOffers = response.data.data.items;
                }
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Failed to fetch public offers';
            } finally {
                this.loading = false;
            }
        },

        async createOffer(data: CreateOfferDto) {
            this.loading = true;
            this.error = null;
            try {
                const response = await offerService.createOffer(data);
                if (response.data.success) {
                    await this.fetchOffers(1, this.pageSize);
                    return true;
                }
                return false;
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Failed to create offer';
                throw err;
            } finally {
                this.loading = false;
            }
        },

        async updateOffer(id: string, data: Partial<CreateOfferDto>) {
            this.loading = true;
            this.error = null;
            try {
                const response = await offerService.updateOffer(id, data);
                if (response.data.success) {
                    await this.fetchOffers(this.currentPage, this.pageSize);
                    return true;
                }
                return false;
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Failed to update offer';
                throw err;
            } finally {
                this.loading = false;
            }
        },

        async lockOffer(id: string) {
            try {
                const response = await offerService.lockOffer(id);
                if (response.data.success) {
                    const offer = this.offers.find(o => o.id === id);
                    if (offer) offer.status = 'LOCKED';
                    return true;
                }
                return false;
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Failed to lock offer';
                throw err;
            }
        }
    }
});
