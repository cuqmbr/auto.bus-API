using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public class ReviewManagementService : IReviewManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<Review> _reviewSortHelper;
    private readonly IDataShaper<Review> _reviewDataShaper;

    public ReviewManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<Review> reviewSortHelper, 
        IDataShaper<Review> reviewDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _reviewSortHelper = reviewSortHelper;
        _reviewDataShaper = reviewDataShaper;
    }

    public async Task<(bool isSucceed, string message, ReviewDto review)> AddReview(CreateReviewDto createReviewDto)
    {
        var review = _mapper.Map<Review>(createReviewDto);
    
        await _dbContext.Reviews.AddAsync(review);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<ReviewDto>(review));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<ReviewDto> reviews,
            PagingMetadata<Review> pagingMetadata)> GetReviews(ReviewParameters parameters)
    {
        var dbReviews = _dbContext.Reviews
            .AsQueryable();

        FilterByReviewRating(ref dbReviews, parameters.FromRating, parameters.ToRating);
        FilterByReviewComment(ref dbReviews, parameters.Comment);
        FilterByReviewUserId(ref dbReviews, parameters.UserId);

        try
        {
            dbReviews = _reviewSortHelper.ApplySort(dbReviews, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbReviews.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbReviews, parameters.PageNumber,
            parameters.PageSize);

        var shapedReviewsData = _reviewDataShaper.ShapeData(dbReviews, parameters.Fields);
        var reviewDtos = shapedReviewsData.ToList().ConvertAll(r => _mapper.Map<ReviewDto>(r));
        
        return (true, "", reviewDtos, pagingMetadata);

        void FilterByReviewRating(ref IQueryable<Review> reviews,
            int? fromRating, int? toRating)
        {
            if (!reviews.Any() || !fromRating.HasValue && !toRating.HasValue)
            {
                return;
            }

            reviews = reviews.Where(r =>
                r.Rating >= fromRating && r.Rating <= toRating);
        }
        
        void FilterByReviewComment(ref IQueryable<Review> reviews,
            string? comment)
        {
            if (!reviews.Any() || String.IsNullOrWhiteSpace(comment))
            {
                return;
            }

            reviews = reviews.Where(r =>
                r.Comment != null &&
                r.Comment.ToLower().Contains(comment.ToLower()));
        }
        
        void FilterByReviewUserId(ref IQueryable<Review> reviews,
            string? userId)
        {
            if (!reviews.Any() || String.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            reviews = reviews.Where(r =>
                r.UserId.Contains(userId.ToLower()));
        }

        PagingMetadata<Review> ApplyPaging(ref IQueryable<Review> reviews,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<Review>(reviews,
                pageNumber, pageSize);
            
            reviews = reviews
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, ReviewDto review)> GetReview(int id, string? fields)
    {
        var dbReview = await _dbContext.Reviews.Where(r => r.Id == id)
            .FirstOrDefaultAsync();

        if (dbReview == null)
        {
            return (false, $"Review doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = ReviewParameters.DefaultFields;
        }
        
        var shapedReviewData = _reviewDataShaper.ShapeData(dbReview, fields);
        var reviewDto = _mapper.Map<ReviewDto>(shapedReviewData);

        return (true, "", reviewDto);
    }

    public async Task<(bool isSucceed, string message, UpdateReviewDto review)> UpdateReview(UpdateReviewDto updateReviewDto)
    {
        var review = _mapper.Map<Review>(updateReviewDto);
        _dbContext.Entry(review).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsReviewExists(updateReviewDto.Id))
            {
                return (false, $"Review with id:{updateReviewDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbReview = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == review.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateReviewDto>(dbReview));
    }

    public async Task<(bool isSucceed, string message)> DeleteReview(int id)
    {
        var dbReview = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id);
    
        if (dbReview == null)
        {
            return (false, $"Review with id:{id} doesn't exist");
        }
    
        _dbContext.Reviews.Remove(dbReview);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsReviewExists(int id)
    {
        return await _dbContext.Reviews.AnyAsync(r => r.Id == id);
    } 
}