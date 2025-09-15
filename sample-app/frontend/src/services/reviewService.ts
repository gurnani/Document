import { api } from '@/lib/api';
import { Review, ProductReviewForm, PagedResponse } from '@/types';

export const reviewService = {
  getProductReviews: async (productId: number, page: number = 1, pageSize: number = 10): Promise<Review[]> => {
    const response = await api.getPaged<Review>('/reviews', {
      productId,
      page,
      pageSize,
    });
    return response.data;
  },

  getReview: async (id: number): Promise<Review> => {
    return api.get<Review>(`/reviews/${id}`);
  },

  createReview: async (productId: number, reviewData: ProductReviewForm): Promise<Review> => {
    return api.post<Review>('/reviews', {
      productId,
      ...reviewData,
    });
  },

  updateReview: async (id: number, reviewData: Partial<ProductReviewForm>): Promise<Review> => {
    return api.put<Review>(`/reviews/${id}`, reviewData);
  },

  deleteReview: async (id: number): Promise<void> => {
    return api.delete<void>(`/reviews/${id}`);
  },

  markHelpful: async (id: number): Promise<void> => {
    return api.post<void>(`/reviews/${id}/helpful`);
  },

  reportReview: async (id: number, reason: string): Promise<void> => {
    return api.post<void>(`/reviews/${id}/report`, { reason });
  },

  getUserReviews: async (userId?: number): Promise<Review[]> => {
    const endpoint = userId ? `/reviews/user/${userId}` : '/reviews/my-reviews';
    return api.get<Review[]>(endpoint);
  },
};
