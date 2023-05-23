using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IVehicleEnrollmentManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, VehicleEnrollmentDto enrollment)> AddEnrollment(CreateVehicleEnrollmentDto createEnrollmentDto);
    Task<(bool isSucceed, IActionResult? actionResult, VehicleEnrollmentWithDetailsDto enrollment)> AddEnrollmentWithDetails(CreateVehicleEnrollmentWithDetailsDto createEnrollmentDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> enrollments,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetEnrollments(VehicleEnrollmentParameters parameters);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> enrollments,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetEnrollmentsWithDetails(VehicleEnrollmentWithDetailsParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject enrollment)> GetEnrollment(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject enrollment)> GetEnrollmentWithDetails(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, VehicleEnrollmentDto enrollment)> UpdateEnrollment(UpdateVehicleEnrollmentDto updateEnrollmentDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteEnrollment(int id);
    Task<bool> IsEnrollmentExists(int id);
}