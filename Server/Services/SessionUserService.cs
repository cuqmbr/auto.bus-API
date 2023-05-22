using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Server.Constants;
using Server.Data;

namespace Server.Services;

public class SessionUserService : ISessionUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ClaimsPrincipal _userClaimsPrincipal;

    public SessionUserService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _userClaimsPrincipal = httpContextAccessor.HttpContext.User;
    }

    public string GetAuthUserId()
    {
        return _userClaimsPrincipal.Claims.FirstOrDefault(c => c.Properties.Values.Any(v => v == JwtStandardClaimNames.Sub))?.Value!;
    }
    
    public string GetAuthUserRole()
    {
        return _userClaimsPrincipal.Claims.FirstOrDefault(c => c.Properties.Values.Any(v => v == "roles"))?.Value!;
    }

    public async Task<(bool isCompanyOwner, int companyId)> IsAuthUserCompanyOwner()
    {
        var companyId = (await _dbContext.Companies.FirstOrDefaultAsync(c => c.OwnerId == GetAuthUserId()))?.Id;

        if (companyId == null)
        {
            return (false, -1);
        }
        
        return (true, (int) companyId);
    }

    public async Task<bool> IsAuthUserCompanyVehicle(int vehicleId)
    {
        var result = await IsAuthUserCompanyOwner();
        if (!result.isCompanyOwner)
        {
            return false;
        }

        if (!await _dbContext.Vehicles.AnyAsync(v => v.Id == vehicleId))
        {
            return true;
        }
        
        return (await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId))?.CompanyId == result.companyId;
    }

    public async Task<bool> IsAuthUserCompanyVehicleEnrollment(int enrollmentId)
    {
        var result = await IsAuthUserCompanyOwner();
        if (!result.isCompanyOwner)
        {
            return false;
        }

        if (!await _dbContext.VehicleEnrollments.AnyAsync(e => e.Id == enrollmentId))
        {
            return true;
        }
        
        return (await _dbContext.VehicleEnrollments
                .Include(e => e.Vehicle)
                .FirstAsync(e => e.Id == enrollmentId))
            .Vehicle.CompanyId == result.companyId;
    }

    public async Task<bool> IsAuthUserCompanyDriver(string driverId)
    {
        var result = await IsAuthUserCompanyOwner();
        if (!result.isCompanyOwner)
        {
            return false;
        }

        if (!await _dbContext.CompanyDrivers.AnyAsync(d => d.DriverId == driverId))
        {
            return true;
        }

        return (await _dbContext.CompanyDrivers
                .FirstAsync(d => d.DriverId == driverId))
            .CompanyId == result.companyId;
    }

    public async Task<bool> IsAuthUserReview(int reviewId)
    {
        return (await _dbContext.Reviews.FirstAsync(r => r.Id == reviewId)).UserId == GetAuthUserId();
    }
}