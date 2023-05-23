using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;
using Utils;

namespace Server.Services;

public class ReviewManagementService : IReviewManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _reviewSortHelper;
    private readonly IDataShaper<ReviewDto> _reviewDataShaper;
    private readonly IPager<ExpandoObject> _pager;
    private readonly ISessionUserService _sessionUserService;

    public ReviewManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> reviewSortHelper, 
        IDataShaper<ReviewDto> reviewDataShaper, IPager<ExpandoObject> pager,
        ISessionUserService sessionUserService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _reviewSortHelper = reviewSortHelper;
        _reviewDataShaper = reviewDataShaper;
        _pager = pager;
        _sessionUserService = sessionUserService;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, ReviewDto review)> AddReview(CreateReviewDto createReviewDto)
    {
        var review = _mapper.Map<Review>(createReviewDto);

        var hasTicketToEnrollment = await _dbContext.TicketGroups.AnyAsync(tg =>
            tg.UserId == _sessionUserService.GetAuthUserId() && tg.Tickets.Any(t => t.VehicleEnrollmentId == review.VehicleEnrollmentId));

        if (!hasTicketToEnrollment && _sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            return (false, new UnauthorizedResult(), null!);
        }
    
        await _dbContext.Reviews.AddAsync(review);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<ReviewDto>(review));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> reviews,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetReviews(ReviewParameters parameters)
    {
        var dbReviews = _dbContext.Reviews
            .Include(r => r.VehicleEnrollment).ThenInclude(ve => ve.Vehicle)
            .ThenInclude(v => v.Company).Include(r => r.User)
            .AsQueryable();
        
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            dbReviews = dbReviews.Where(r => r.UserId == _sessionUserService.GetAuthUserId());
        }

        if (!dbReviews.Any())
        {
            return (false, new NotFoundResult(), null!, null!);
        }

        FilterByReviewRating(ref dbReviews, parameters.FromRating, parameters.ToRating);
        FilterByReviewComment(ref dbReviews, parameters.Comment);
        FilterByReviewUserId(ref dbReviews, parameters.UserId);
        FilterByReviewCompanyId(ref dbReviews, parameters.CompanyId);

        var reviewDtos = _mapper.ProjectTo<ReviewDto>(dbReviews);
        var shapedData = _reviewDataShaper.ShapeData(reviewDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _reviewSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception e)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

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
        
        void FilterByReviewCompanyId(ref IQueryable<Review> reviews, int? companyId)
        {
            if (!reviews.Any() || companyId == null)
            {
                return;
            }

            reviews = reviews.Where(r => r.VehicleEnrollment.Vehicle.CompanyId == companyId);
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject review)> GetReview(int id, string? fields)
    {
        if (!await IsReviewExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbReview = await _dbContext.Reviews.Where(r => r.Id == id)
            .Include(r => r.VehicleEnrollment).ThenInclude(ve => ve.Vehicle)
            .ThenInclude(v => v.Company).Include(r => r.User)
            .FirstAsync();
        
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString() &&
            dbReview.UserId != _sessionUserService.GetAuthUserId())
        {
            return (false, new UnauthorizedResult(), null!);
        }

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = ReviewParameters.DefaultFields;
        }
        
        var reviewDto = _mapper.Map<ReviewDto>(dbReview);
        var shapedData = _reviewDataShaper.ShapeData(reviewDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, ReviewDto review)> UpdateReview(UpdateReviewDto updateReviewDto)
    {
        var review = _mapper.Map<Review>(updateReviewDto);
        _dbContext.Entry(review).State = EntityState.Modified;
        
        if (!await _sessionUserService.IsAuthUserReview(updateReviewDto.Id) &&
            _sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            return (false, new UnauthorizedResult(), null!);
        }
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsReviewExists(updateReviewDto.Id))
            {
                return (false, new NotFoundResult(), null!);
            }
        }

        var dbReview = await _dbContext.Reviews.FirstAsync(r => r.Id == review.Id);
        
        return (true, null, _mapper.Map<ReviewDto>(dbReview));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteReview(int id)
    {
        if (!await IsReviewExists(id))
        {
            return (false, new NotFoundResult());
        }
        
        if (!await _sessionUserService.IsAuthUserReview(id))
        {
            return (false, new UnauthorizedResult());
        }
        
        var dbReview = await _dbContext.Reviews.FirstAsync(r => r.Id == id);
    
        _dbContext.Reviews.Remove(dbReview);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsReviewExists(int id)
    {
        return await _dbContext.Reviews.AnyAsync(r => r.Id == id);
    } 
}