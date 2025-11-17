using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DepiFinalProject.DTOs.Reviews;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using DepiFinalProject.Repositories;
using PaypalServerSdk.Standard.Models;

namespace DepiFinalProject.Services
{
    public class ReviewService : IReviewService
    {
        protected readonly IReviewRepository _reviewRepository;
        protected readonly IOrderRepository _orderRepository;
    
        public ReviewService(IReviewRepository reviewRepository, IOrderRepository orderRepository)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;

        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductIdAsync(int productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
            return reviews.Select(r => new ReviewResponseDto
            {
                ReviewID = r.ReviewID,
                ProductID = r.ProductID,
                UserName = r.User?.UserFirstName + ' '+ r.User?.UserLastName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task<ReviewResponseDto> AddReviewAsync(int userId, ReviewCreateDto dto)
        {
            ValidateRating(dto.Rating);
            if(!await _orderRepository.HasuserorderedproductAsync(userId, dto.ProductID))
                throw new BadHttpRequestException("User has not purchased this product.");
            if (await _reviewRepository.HasUserReviewedProductAsync(userId, dto.ProductID))
                throw new BadHttpRequestException("User has already reviewed this product.");


            var review = new Review
            {
                ProductID = dto.ProductID,
                UserID = userId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            var created = await _reviewRepository.AddReviewAsync(review);

            return new ReviewResponseDto
            {
                ReviewID = created.ReviewID,
                ProductID = created.ProductID,
                Rating = created.Rating,
                Comment = created.Comment,
                CreatedAt = created.CreatedAt,
                UserName = created.User?.UserFirstName + ' ' + created.User?.UserLastName

            };
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(int id, int userId, ReviewUpdateDto dto, bool isAdmin)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                throw new KeyNotFoundException("Review not found.");

            if (!isAdmin && review.UserID != userId)
                throw new UnauthorizedAccessException("You can only edit your own reviews.");

            ValidateRating(dto.Rating);


            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            var updated = await _reviewRepository.UpdateReviewAsync(review);

            return new ReviewResponseDto
            {
                ReviewID = updated.ReviewID,
                ProductID = updated.ProductID,
                Rating = updated.Rating,
                Comment = updated.Comment,
                CreatedAt = updated.CreatedAt,
                UserName= updated.User?.UserFirstName + ' ' + updated.User?.UserLastName
            };
        }


        public async Task<bool> DeleteReviewAsync(int id, int userId, bool isAdmin)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) return false;

            if (!isAdmin && review.UserID != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews.");

            return await _reviewRepository.DeleteReviewAsync(id);
        }
        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByUserIdAsync(int userid)
        {
            var reviews= await _reviewRepository.getreviewsbyuser(userid);
            return reviews.Select(r => new ReviewResponseDto
            {
                ReviewID = r.ReviewID,
                ProductID = r.ProductID,
                UserName = r.User?.UserFirstName + ' ' + r.User?.UserLastName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            });
        }
        private void ValidateRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");
        }

    }
}
