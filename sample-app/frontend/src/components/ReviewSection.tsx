import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Star, ThumbsUp, Flag } from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { reviewService } from '@/services/reviewService';
import { ProductReviewForm, Review } from '@/types';
import { LoadingSpinner } from './LoadingSpinner';
import toast from 'react-hot-toast';

interface ReviewSectionProps {
  productId: number;
}

export const ReviewSection = ({ productId }: ReviewSectionProps) => {
  const { user, isAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const [showReviewForm, setShowReviewForm] = useState(false);
  const [reviewForm, setReviewForm] = useState<ProductReviewForm>({
    rating: 5,
    title: '',
    comment: '',
  });

  const { data: reviews, isLoading } = useQuery({
    queryKey: ['reviews', productId],
    queryFn: () => reviewService.getProductReviews(productId),
  });

  const createReviewMutation = useMutation({
    mutationFn: (review: ProductReviewForm) => reviewService.createReview(productId, review),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reviews', productId] });
      setShowReviewForm(false);
      setReviewForm({ rating: 5, title: '', comment: '' });
      toast.success('Review submitted successfully!');
    },
    onError: (error: any) => {
      toast.error(error.message || 'Failed to submit review');
    },
  });

  const handleSubmitReview = (e: React.FormEvent) => {
    e.preventDefault();
    if (!reviewForm.title.trim() || !reviewForm.comment.trim()) {
      toast.error('Please fill in all fields');
      return;
    }
    createReviewMutation.mutate(reviewForm);
  };

  const renderStars = (rating: number, interactive = false, onRatingChange?: (rating: number) => void) => {
    return (
      <div className="flex items-center">
        {[1, 2, 3, 4, 5].map((star) => (
          <button
            key={star}
            type={interactive ? 'button' : undefined}
            onClick={interactive && onRatingChange ? () => onRatingChange(star) : undefined}
            className={interactive ? 'cursor-pointer' : 'cursor-default'}
          >
            <Star
              className={`w-5 h-5 ${
                star <= rating
                  ? 'text-yellow-400 fill-current'
                  : 'text-gray-300'
              } ${interactive ? 'hover:text-yellow-400' : ''}`}
            />
          </button>
        ))}
      </div>
    );
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  const averageRating = reviews?.length 
    ? reviews.reduce((sum, review) => sum + review.rating, 0) / reviews.length 
    : 0;

  return (
    <section className="mt-12">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Customer Reviews</h2>
          {reviews && reviews.length > 0 && (
            <div className="flex items-center mt-2">
              {renderStars(Math.round(averageRating))}
              <span className="ml-2 text-sm text-gray-600">
                {averageRating.toFixed(1)} out of 5 ({reviews.length} reviews)
              </span>
            </div>
          )}
        </div>

        {isAuthenticated && (
          <button
            onClick={() => setShowReviewForm(!showReviewForm)}
            className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700"
          >
            Write a Review
          </button>
        )}
      </div>

      {/* Review Form */}
      {showReviewForm && (
        <div className="bg-gray-50 rounded-lg p-6 mb-8">
          <h3 className="text-lg font-semibold mb-4">Write Your Review</h3>
          <form onSubmit={handleSubmitReview} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Rating
              </label>
              {renderStars(reviewForm.rating, true, (rating) => 
                setReviewForm(prev => ({ ...prev, rating }))
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Review Title
              </label>
              <input
                type="text"
                value={reviewForm.title}
                onChange={(e) => setReviewForm(prev => ({ ...prev, title: e.target.value }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Summarize your review"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Your Review
              </label>
              <textarea
                value={reviewForm.comment}
                onChange={(e) => setReviewForm(prev => ({ ...prev, comment: e.target.value }))}
                rows={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Tell others about your experience with this product"
                required
              />
            </div>

            <div className="flex space-x-3">
              <button
                type="submit"
                disabled={createReviewMutation.isPending}
                className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                {createReviewMutation.isPending ? 'Submitting...' : 'Submit Review'}
              </button>
              <button
                type="button"
                onClick={() => setShowReviewForm(false)}
                className="px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Reviews List */}
      {reviews && reviews.length > 0 ? (
        <div className="space-y-6">
          {reviews.map((review) => (
            <div key={review.id} className="border-b border-gray-200 pb-6">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <div className="flex items-center mb-1">
                    {renderStars(review.rating)}
                    <span className="ml-2 font-medium text-gray-900">
                      {review.title}
                    </span>
                  </div>
                  <div className="flex items-center text-sm text-gray-600">
                    <span>{review.user.firstName} {review.user.lastName}</span>
                    <span className="mx-2">•</span>
                    <span>{new Date(review.createdAt).toLocaleDateString()}</span>
                    {review.isVerifiedPurchase && (
                      <>
                        <span className="mx-2">•</span>
                        <span className="text-green-600 font-medium">Verified Purchase</span>
                      </>
                    )}
                  </div>
                </div>
              </div>

              <p className="text-gray-700 mb-4 leading-relaxed">
                {review.comment}
              </p>

              <div className="flex items-center space-x-4 text-sm">
                <button className="flex items-center text-gray-600 hover:text-gray-900">
                  <ThumbsUp className="w-4 h-4 mr-1" />
                  Helpful ({review.helpfulCount})
                </button>
                <button className="flex items-center text-gray-600 hover:text-gray-900">
                  <Flag className="w-4 h-4 mr-1" />
                  Report
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-12">
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            No reviews yet
          </h3>
          <p className="text-gray-600">
            Be the first to review this product!
          </p>
        </div>
      )}
    </section>
  );
};
