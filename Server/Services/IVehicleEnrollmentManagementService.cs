using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IVehicleEnrollmentManagementService
{
    Task<(bool isSucceed, IActionResult actionResult, VehicleEnrollmentDto enrollment)>
        AddEnrollment(CreateVehicleEnrollmentDto createEnrollmentDto);
    
    Task<(bool isSucceed, IActionResult actionResult, IEnumerable<ExpandoObject> enrollments, PagingMetadata<ExpandoObject> pagingMetadata)>
        GetEnrollments(VehicleEnrollmentParameters parameters); 
    
    Task<(bool isSucceed, IActionResult actionResult, ExpandoObject enrollment)>
        GetEnrollment(int id, string? fields);
    
    Task<(bool isSucceed, IActionResult actionResult, VehicleEnrollmentDto enrollment)>
        UpdateEnrollment(int vehicleEnrollmentId, UpdateVehicleEnrollmentDto updateEnrollmentDto);
    
    Task<(bool isSucceed, IActionResult actionResult)> DeleteEnrollment(int id);
}