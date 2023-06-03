namespace Server.Services;

public interface ISessionUserService
{
    public string GetAuthUserId();
    public string GetAuthUserRole();
    
    public Task<(bool isCompanyOwner, int companyId)> IsAuthUserCompanyOwner();
    public Task<bool> IsAuthUserCompanyVehicle(int vehicleId);
    public Task<bool> IsAuthUserCompanyVehicleEnrollment(int enrollmentId);
    public Task<bool> IsAuthUserCompanyDriver(string driverId);

    public Task<bool> IsAuthUserReview(int reviewId);
}