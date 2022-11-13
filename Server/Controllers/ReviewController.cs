using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;

[Route("api/reviews")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewManagementService _reviewManagementService;
    
    public ReviewController(IReviewManagementService reviewManagementService)
    {
        _reviewManagementService = reviewManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddReview(CreateReviewDto review)
    {
        var result = await _reviewManagementService.AddReview(review);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetReview), new {id = result.review.Id}, result.review);
    }

    [HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] ReviewParameters parameters)
    {
        var result = await _reviewManagementService.GetReviews(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.reviews);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReview(int id, [FromQuery] string? fields)
    {
        if (!await _reviewManagementService.IsReviewExists(id))
        {
            return NotFound();
        }

        var result = await _reviewManagementService.GetReview(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.review);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto review)
    {
        if (id != review.Id)
        {
            return BadRequest();
        }
        
        var result = await _reviewManagementService.UpdateReview(review);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.review);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        if (!await _reviewManagementService.IsReviewExists(id))
        {
            return NotFound();
        }
        
        var result = await _reviewManagementService.DeleteReview(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}