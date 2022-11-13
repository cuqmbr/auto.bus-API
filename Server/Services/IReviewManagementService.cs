using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public interface IReviewManagementService
{
    Task<(bool isSucceed, string message, ReviewDto review)> AddReview(CreateReviewDto createReviewDto);
    Task<(bool isSucceed, string message, IEnumerable<ReviewDto> reviews,
        PagingMetadata<Review> pagingMetadata)> GetReviews(ReviewParameters parameters);
    Task<(bool isSucceed, string message, ReviewDto review)> GetReview(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateReviewDto review)> UpdateReview(UpdateReviewDto updateReviewDto);
    Task<(bool isSucceed, string message)> DeleteReview(int id);
    Task<bool> IsReviewExists(int id);
}