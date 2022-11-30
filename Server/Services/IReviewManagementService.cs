using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IReviewManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, ReviewDto review)> AddReview(CreateReviewDto createReviewDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> reviews,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetReviews(ReviewParameters parameters);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject review)> GetReview(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, ReviewDto review)> UpdateReview(UpdateReviewDto updateReviewDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteReview(int id);
    Task<bool> IsReviewExists(int id);
}