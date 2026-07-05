import api from './api';

export interface ReviewDto {
    id: number;
    customerId: string;
    customerName: string;
    customerAvatarUrl?: string;
    appointmentId: number;
    serviceName?: string;
    rating: number;
    comment?: string;
    createdAt: string;
    deletedAt?: string | null;
}

export interface CreateReviewDto {
    appointmentId: number;
    rating: number;
    comment?: string;
}

export interface UpdateReviewDto {
    rating: number;
    comment?: string;
}

export interface ReviewStatisticsDto {
    total: number;
    avgRating: number;
    ratingDistribution: Record<number, number>;
}

export interface PaginatedReviews {
    items: ReviewDto[];
    totalItems: number;
    page: number;
    limit: number;
    totalPages: number;
}

class ReviewService {
    async getPublicReviews(page: number = 1, limit: number = 10, sortBy?: string, rating?: number): Promise<PaginatedReviews> {
        const params = new URLSearchParams({ page: page.toString(), limit: limit.toString() });
        if (sortBy) params.append('sortBy', sortBy);
        if (rating) params.append('rating', rating.toString());
        
        const response = await api.get(`/Reviews?${params.toString()}`);
        return response.data;
    }

    async getMyReviews(page: number = 1, limit: number = 10): Promise<PaginatedReviews> {
        const response = await api.get(`/Reviews/my-reviews?page=${page}&limit=${limit}`);
        return response.data;
    }

    async createReview(dto: CreateReviewDto): Promise<ReviewDto> {
        const response = await api.post('/Reviews', dto);
        return response.data;
    }

    async updateReview(id: number, dto: UpdateReviewDto): Promise<ReviewDto> {
        const response = await api.put(`/Reviews/${id}`, dto);
        return response.data;
    }

    async getStatistics(): Promise<ReviewStatisticsDto> {
        const response = await api.get('/Reviews/statistics');
        return response.data;
    }

    async getAllAdminReviews(page: number = 1, limit: number = 10): Promise<PaginatedReviews> {
        const response = await api.get(`/Reviews/all?page=${page}&limit=${limit}`);
        return response.data;
    }

    async softDeleteReview(id: number): Promise<void> {
        await api.delete(`/Reviews/${id}`);
    }

    async restoreReview(id: number): Promise<void> {
        await api.patch(`/Reviews/${id}/restore`);
    }
}

export default new ReviewService();
