using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IVehicleEnrollmentManagementService
{
    Task<(bool isSucceed, string message, VehicleEnrollmentDto enrollment)> AddEnrollment(CreateVehicleEnrollmentDto createEnrollmentDto);
    Task<(bool isSucceed, string message, IEnumerable<VehicleEnrollmentDto> enrollments,
        PagingMetadata<VehicleEnrollment> pagingMetadata)> GetEnrollments(VehicleEnrollmentParameters parameters); 
    Task<(bool isSucceed, string message, VehicleEnrollmentDto enrollment)> GetEnrollment(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateVehicleEnrollmentDto enrollment)> UpdateEnrollment(UpdateVehicleEnrollmentDto updateEnrollmentDto);
    Task<(bool isSucceed, string message)> DeleteEnrollment(int id);
    Task<bool> IsEnrollmentExists(int id);
}