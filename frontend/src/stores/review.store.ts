import { defineStore } from 'pinia';
import type { ReviewDto, ReviewStatisticsDto, CreateReviewDto, UpdateReviewDto, PaginatedReviews } from '../services/review.service';
import ReviewService from '../services/review.service';

export const useReviewStore = defineStore('review', {
    state: () => ({
        publicReviews: [] as ReviewDto[],
        myReviews: [] as ReviewDto[],
        adminReviews: [] as ReviewDto[],
        statistics: null as ReviewStatisticsDto | null,
        loading: false,
        error: null as string | null,
        publicPagination: { page: 1, limit: 10, totalPages: 1, totalItems: 0 },
        myPagination: { page: 1, limit: 10, totalPages: 1, totalItems: 0 },
        adminPagination: { page: 1, limit: 10, totalPages: 1, totalItems: 0 },
    }),

    actions: {
        async fetchPublicReviews(page: number = 1, sortBy?: string, rating?: number) {
            this.loading = true;
            this.error = null;
            try {
                const res = await ReviewService.getPublicReviews(page, this.publicPagination.limit, sortBy, rating);
                this.publicReviews = res.items;
                this.publicPagination = {
                    page: res.page,
                    limit: res.limit,
                    totalPages: res.totalPages,
                    totalItems: res.totalItems
                };
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Lỗi khi tải danh sách đánh giá.';
            } finally {
                this.loading = false;
            }
        },

        async fetchMyReviews(page: number = 1) {
            this.loading = true;
            this.error = null;
            try {
                const res = await ReviewService.getMyReviews(page, this.myPagination.limit);
                this.myReviews = res.items;
                this.myPagination = {
                    page: res.page,
                    limit: res.limit,
                    totalPages: res.totalPages,
                    totalItems: res.totalItems
                };
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Lỗi khi tải danh sách đánh giá của bạn.';
            } finally {
                this.loading = false;
            }
        },

        async submitReview(dto: CreateReviewDto) {
            this.loading = true;
            this.error = null;
            try {
                await ReviewService.createReview(dto);
                await this.fetchMyReviews(1); // Refresh list
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Lỗi khi gửi đánh giá.';
                throw err;
            } finally {
                this.loading = false;
            }
        },

        async updateMyReview(id: number, dto: UpdateReviewDto) {
            this.loading = true;
            this.error = null;
            try {
                await ReviewService.updateReview(id, dto);
                await this.fetchMyReviews(this.myPagination.page);
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Lỗi khi cập nhật đánh giá.';
                throw err;
            } finally {
                this.loading = false;
            }
        },

        // Admin actions
        async fetchAdminReviews(page: number = 1) {
            this.loading = true;
            this.error = null;
            try {
                const res = await ReviewService.getAllAdminReviews(page, this.adminPagination.limit);
                this.adminReviews = res.items;
                this.adminPagination = {
                    page: res.page,
                    limit: res.limit,
                    totalPages: res.totalPages,
                    totalItems: res.totalItems
                };
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Lỗi tải danh sách quản lý.';
            } finally {
                this.loading = false;
            }
        },

        async fetchStatistics() {
            try {
                this.statistics = await ReviewService.getStatistics();
            } catch (err: any) {
                console.error("Lỗi lấy thống kê", err);
            }
        },

        async softDeleteReview(id: number) {
            try {
                await ReviewService.softDeleteReview(id);
                await this.fetchAdminReviews(this.adminPagination.page);
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Lỗi khi xóa đánh giá.';
                throw err;
            }
        },

        async restoreReview(id: number) {
            try {
                await ReviewService.restoreReview(id);
                await this.fetchAdminReviews(this.adminPagination.page);
            } catch (err: any) {
                this.error = err.response?.data?.message || 'Lỗi khi khôi phục đánh giá.';
                throw err;
            }
        }
    }
});
