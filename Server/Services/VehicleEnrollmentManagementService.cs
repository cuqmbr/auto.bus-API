using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public class VehicleEnrollmentManagementService : IVehicleEnrollmentManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<VehicleEnrollment> _enrollmentSortHelper;
    private readonly IDataShaper<VehicleEnrollment> _enrollmentDataShaper;

    public VehicleEnrollmentManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<VehicleEnrollment> enrollmentSortHelper, 
        IDataShaper<VehicleEnrollment> enrollmentDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _enrollmentSortHelper = enrollmentSortHelper;
        _enrollmentDataShaper = enrollmentDataShaper;
    }

    public async Task<(bool isSucceed, string message, VehicleEnrollmentDto enrollment)> AddEnrollment(CreateVehicleEnrollmentDto createEnrollmentDto)
    {
        var enrollment = _mapper.Map<VehicleEnrollment>(createEnrollmentDto);
    
        await _dbContext.VehicleEnrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<VehicleEnrollmentDto>(enrollment));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<VehicleEnrollmentDto> enrollments,
            PagingMetadata<VehicleEnrollment> pagingMetadata)> GetEnrollments(VehicleEnrollmentParameters parameters)
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

        try
        {
            dbEnrollments = _enrollmentSortHelper.ApplySort(dbEnrollments, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbEnrollments.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbEnrollments, parameters.PageNumber,
            parameters.PageSize);

        var shapedEnrollmentsData = _enrollmentDataShaper.ShapeData(dbEnrollments, parameters.Fields);
        var enrollmentDtos = shapedEnrollmentsData.ToList().ConvertAll(e => _mapper.Map<VehicleEnrollmentDto>(e));
        
        return (true, "", enrollmentDtos, pagingMetadata);

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

        PagingMetadata<VehicleEnrollment> ApplyPaging(ref IQueryable<VehicleEnrollment> enrollments,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<VehicleEnrollment>(enrollments,
                pageNumber, pageSize);
            
            enrollments = enrollments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, VehicleEnrollmentDto enrollment)> GetEnrollment(int id, string? fields)
    {
        var dbEnrollment = await _dbContext.VehicleEnrollments.Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (dbEnrollment == null)
        {
            return (false, $"Enrollment doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = VehicleEnrollmentParameters.DefaultFields;
        }
        
        var shapedEnrollmentData = _enrollmentDataShaper.ShapeData(dbEnrollment, fields);
        var enrollmentDto = _mapper.Map<VehicleEnrollmentDto>(shapedEnrollmentData);

        return (true, "", enrollmentDto);
    }

    public async Task<(bool isSucceed, string message, UpdateVehicleEnrollmentDto enrollment)> UpdateEnrollment(UpdateVehicleEnrollmentDto updateEnrollmentDto)
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
                return (false, $"Enrollment with id:{updateEnrollmentDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbEnrollment = await _dbContext.VehicleEnrollments.FirstOrDefaultAsync(e => e.Id == enrollment.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateVehicleEnrollmentDto>(dbEnrollment));
    }

    public async Task<(bool isSucceed, string message)> DeleteEnrollment(int id)
    {
        var dbEnrollment = await _dbContext.VehicleEnrollments.FirstOrDefaultAsync(e => e.Id == id);
    
        if (dbEnrollment == null)
        {
            return (false, $"Enrollment with id:{id} doesn't exist");
        }
    
        _dbContext.VehicleEnrollments.Remove(dbEnrollment);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsEnrollmentExists(int id)
    {
        return await _dbContext.VehicleEnrollments.AnyAsync(e => e.Id == id);
    } 
}