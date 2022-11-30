using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public class VehicleEnrollmentManagementService : IVehicleEnrollmentManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _enrollmentSortHelper;
    private readonly IDataShaper<VehicleEnrollmentDto> _enrollmentDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public VehicleEnrollmentManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> enrollmentSortHelper, 
        IDataShaper<VehicleEnrollmentDto> enrollmentDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _enrollmentSortHelper = enrollmentSortHelper;
        _enrollmentDataShaper = enrollmentDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, VehicleEnrollmentDto enrollment)> AddEnrollment(CreateVehicleEnrollmentDto createEnrollmentDto)
    {
        var enrollment = _mapper.Map<VehicleEnrollment>(createEnrollmentDto);
    
        await _dbContext.VehicleEnrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<VehicleEnrollmentDto>(enrollment));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> enrollments,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetEnrollments(VehicleEnrollmentParameters parameters)
    {
        var dbEnrollments = _dbContext.VehicleEnrollments
            .AsQueryable();

        SearchByAllEnrollmentFields(ref dbEnrollments, parameters.Search);
        FilterByEnrollmentVehicleId(ref dbEnrollments, parameters.VehicleId);
        FilterByEnrollmentRouteId(ref dbEnrollments, parameters.RouteId);
        FilterByEnrollmentDepartureDateTime(ref dbEnrollments,
            parameters.FromDepartureDateTime, parameters.ToDepartureDateTime);
        FilterByEnrollmentDelayedValue(ref dbEnrollments, parameters.IsDelayed);
        FilterByEnrollmentCancelledValue(ref dbEnrollments, parameters.IsCanceled);

        var enrollmentDtos = _mapper.ProjectTo<VehicleEnrollmentDto>(dbEnrollments);
        var shapedData = _enrollmentDataShaper.ShapeData(enrollmentDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _enrollmentSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

        void SearchByAllEnrollmentFields(ref IQueryable<VehicleEnrollment> enrollment,
            string? search)
        {
            if (!enrollment.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            enrollment = enrollment.Where(e =>
                e.CancelationComment.ToLower().Contains(search.ToLower()));
        }
        
        void FilterByEnrollmentVehicleId(ref IQueryable<VehicleEnrollment> enrollments,
            int? vehicleId)
        {
            if (!enrollments.Any() || vehicleId == null)
            {
                return;
            }

            enrollments = enrollments.Where(e => e.VehicleId == vehicleId);
        }
        
        void FilterByEnrollmentRouteId(ref IQueryable<VehicleEnrollment> enrollments,
            int? routeId)
        {
            if (!enrollments.Any() || routeId == null)
            {
                return;
            }

            enrollments = enrollments.Where(e => e.RouteId == routeId);
        }
        
        void FilterByEnrollmentDepartureDateTime(ref IQueryable<VehicleEnrollment> enrollments,
            DateTime? fromDateTime, DateTime? toDateTime)
        {
            if (!enrollments.Any() || fromDateTime == null || toDateTime == null)
            {
                return;
            }

            enrollments = enrollments.Where(e =>
                e.DepartureDateTimeUtc >= fromDateTime.Value.ToUniversalTime() &&
                e.DepartureDateTimeUtc <= toDateTime.Value.ToUniversalTime());
        }
        
        void FilterByEnrollmentDelayedValue(ref IQueryable<VehicleEnrollment> enrollments,
            bool? isDelayed)
        {
            if (!enrollments.Any() || !isDelayed.HasValue)
            {
                return;
            }

            enrollments = isDelayed.Value
                ? enrollments.Where(e => e.DelayTimeSpan != null)
                : enrollments.Where(e => e.DelayTimeSpan == null);
        }
        
        void FilterByEnrollmentCancelledValue(ref IQueryable<VehicleEnrollment> enrollments,
            bool? isCancelled)
        {
            if (!enrollments.Any() || !isCancelled.HasValue)
            {
                return;
            }
            
            enrollments = enrollments.Where(e => e.IsCanceled == isCancelled);
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject enrollment)> GetEnrollment(int id, string? fields)
    {
        if (!await IsEnrollmentExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbEnrollment = await _dbContext.VehicleEnrollments.Where(e => e.Id == id)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = VehicleEnrollmentParameters.DefaultFields;
        }
        
        var enrollmentDto = _mapper.Map<VehicleEnrollmentDto>(dbEnrollment);
        var shapedData = _enrollmentDataShaper.ShapeData(enrollmentDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, VehicleEnrollmentDto enrollment)> UpdateEnrollment(UpdateVehicleEnrollmentDto updateEnrollmentDto)
    {
        var enrollment = _mapper.Map<VehicleEnrollment>(updateEnrollmentDto);
        _dbContext.Entry(enrollment).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsEnrollmentExists(updateEnrollmentDto.Id))
            {
                return (false, new NotFoundResult(), null!);
            }
            
            throw;
        }

        var dbEnrollment = await _dbContext.VehicleEnrollments.FirstAsync(e => e.Id == enrollment.Id);
        
        return (true, null, _mapper.Map<VehicleEnrollmentDto>(dbEnrollment));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteEnrollment(int id)
    {
        var dbEnrollment = await _dbContext.VehicleEnrollments.FirstOrDefaultAsync(e => e.Id == id);
    
        if (dbEnrollment == null)
        {
            return (false, new NotFoundResult());
        }
    
        _dbContext.VehicleEnrollments.Remove(dbEnrollment);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsEnrollmentExists(int id)
    {
        return await _dbContext.VehicleEnrollments.AnyAsync(e => e.Id == id);
    } 
}