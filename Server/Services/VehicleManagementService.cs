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

public class VehicleManagementService : IVehicleManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _vehicleSortHelper;
    private readonly IDataShaper<VehicleDto> _vehicleDataShaper;
    private readonly IPager<ExpandoObject> _pager;
    private readonly ISessionUserService _sessionUserService;

    public VehicleManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> vehicleSortHelper, 
        IDataShaper<VehicleDto> vehicleDataShaper, IPager<ExpandoObject> pager,
        ISessionUserService sessionUserService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _vehicleSortHelper = vehicleSortHelper;
        _vehicleDataShaper = vehicleDataShaper;
        _pager = pager;
        _sessionUserService = sessionUserService;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, VehicleDto vehicle)> AddVehicle(CreateVehicleDto createVehicleDto)
    {
        if (_sessionUserService.GetAuthUserRole() == Identity.Roles.Administrator.ToString())
        {
            if (createVehicleDto.CompanyId == null)
            {
                return (false, new BadRequestObjectResult("CompanyId must have a value"), null!);
            }
        }
        else
        {
            var result = await _sessionUserService.IsAuthUserCompanyOwner();
            if (!result.isCompanyOwner)
            {
                return (false, new UnauthorizedResult(), null!);
            }
            createVehicleDto.CompanyId = result.companyId;
        }
    
        var vehicle = _mapper.Map<Vehicle>(createVehicleDto);
    
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<VehicleDto>(vehicle));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> vehicles,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetVehicles(VehicleParameters parameters)
    {
        var dbVehicles = _dbContext.Vehicles
            .Include(t => t.Company)
            .AsQueryable();

        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            var result = await _sessionUserService.IsAuthUserCompanyOwner();
            if (!result.isCompanyOwner)
            {
                return (false, new UnauthorizedResult(), null!, null!);
            }
            
            dbVehicles = dbVehicles.Where(v => v.CompanyId == result.companyId);
        }

        if (!dbVehicles.Any())
        {
            return (false, new NotFoundResult(), null!, null!);
        }

        var vehicleDtos = _mapper.ProjectTo<VehicleDto>(dbVehicles);
        var shapedData = _vehicleDataShaper.ShapeData(vehicleDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _vehicleSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

        void SearchByAllVehicleFields(ref IQueryable<Vehicle> vehicle,
            string? search)
        {
            if (!vehicle.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            vehicle = vehicle.Where(v =>
                v.Number.ToLower().Contains(search.ToLower()) ||
                v.Type.ToLower().Contains(search.ToLower()));
        }
        
        void FilterByVehicleNumber(ref IQueryable<Vehicle> vehicles,
            string? number)
        {
            if (!vehicles.Any() || String.IsNullOrWhiteSpace(number))
            {
                return;
            }

            vehicles = vehicles.Where(v => v.Number == number);
        }
        
        void FilterByVehicleType(ref IQueryable<Vehicle> vehicles,
            string? type)
        {
            if (!vehicles.Any() || String.IsNullOrWhiteSpace(type))
            {
                return;
            }

            vehicles = vehicles.Where(v => v.Type == type);
        }
        
        void FilterByVehicleCapacity(ref IQueryable<Vehicle> vehicles,
            int? fromCapacity, int? toCapacity)
        {
            if (!vehicles.Any() || fromCapacity == null && toCapacity == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.Capacity >= fromCapacity &&
                                           v.Capacity <= toCapacity);
        }
        
        void FilterByVehicleClimateControlAvailability(ref IQueryable<Vehicle> vehicles,
            bool? hasClimateControl)
        {
            if (!vehicles.Any() || hasClimateControl == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.HasClimateControl == hasClimateControl);
        }
        
        void FilterByVehicleWiFiAvailability(ref IQueryable<Vehicle> vehicles,
            bool? hasWiFi)
        {
            if (!vehicles.Any() || hasWiFi == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.HasWiFi == hasWiFi);
        }
        
        void FilterByVehicleWCAvailability(ref IQueryable<Vehicle> vehicles,
            bool? hasWC)
        {
            if (!vehicles.Any() || hasWC == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.HasWC == hasWC);
        }
        
        void FilterByStewardessAvailability(ref IQueryable<Vehicle> vehicles,
            bool? hasStewardess)
        {
            if (!vehicles.Any() || hasStewardess == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.HasStewardess == hasStewardess);
        }
        
        void FilterByVehicleTVAvailability(ref IQueryable<Vehicle> vehicles,
            bool? hasTV)
        {
            if (!vehicles.Any() || hasTV == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.HasTV == hasTV);
        }
        
        void FilterByVehicleOutletAvailability(ref IQueryable<Vehicle> vehicles,
            bool? hasOutlet)
        {
            if (!vehicles.Any() || hasOutlet == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.HasOutlet == hasOutlet);
        }
        
        void FilterByVehicleBeltsAvailability(ref IQueryable<Vehicle> vehicles,
            bool? hasBelts)
        {
            if (!vehicles.Any() || hasBelts == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.HasBelts == hasBelts);
        }
        
        void FilterByVehicleCompanyId(ref IQueryable<Vehicle> vehicles,
            int? companyId)
        {
            if (!vehicles.Any() || companyId == null)
            {
                return;
            }

            vehicles = vehicles.Where(v => v.CompanyId == companyId);
        }
        
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject vehicle)> GetVehicle(int id, string? fields)
    {
        if (!await IsVehicleExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }

        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            if (!await _sessionUserService.IsAuthUserCompanyVehicle(id))
            {
                return (false, new UnauthorizedResult(), null!);
            }
        }
        
        var dbVehicle = await _dbContext.Vehicles.Where(v => v.Id == id)
            .Include(t => t.Company)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = VehicleParameters.DefaultFields;
        }
        
        var vehicleDto = _mapper.Map<VehicleDto>(dbVehicle);
        var shapedData = _vehicleDataShaper.ShapeData(vehicleDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, VehicleDto vehicle)> UpdateVehicle(UpdateVehicleDto updateVehicleDto)
    {
        var vehicle = _mapper.Map<Vehicle>(updateVehicleDto);
        _dbContext.Entry(vehicle).State = EntityState.Modified;
        
        if (_sessionUserService.GetAuthUserRole() == Identity.Roles.Administrator.ToString())
        {
            if (updateVehicleDto.CompanyId == null)
            {
                return (false, new BadRequestObjectResult("CompanyId must have a value"), null!);
            }
        }
        else
        {
            var result = await _sessionUserService.IsAuthUserCompanyOwner();
            if (!result.isCompanyOwner)
            {
                return (false, new UnauthorizedResult(), null!);
            }
            vehicle.CompanyId = result.companyId;
        }
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsVehicleExists(updateVehicleDto.Id))
            {
                return (false, new NotFoundResult(), null!);
            }
            
            throw;
        }

        var dbVehicle = await _dbContext.Vehicles.FirstAsync(v => v.Id == vehicle.Id);
        
        return (true, null, _mapper.Map<VehicleDto>(dbVehicle));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteVehicle(int id)
    {
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            if (!await _sessionUserService.IsAuthUserCompanyVehicle(id))
            {
                return (false, new UnauthorizedResult());
            }
        }
        
        if (!await IsVehicleExists(id))
        {
            return (false, new NotFoundResult());
        }

        var dbVehicle = await _dbContext.Vehicles.FirstAsync(v => v.Id == id);
        
        _dbContext.Vehicles.Remove(dbVehicle);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsVehicleExists(int id)
    {
        return await _dbContext.Vehicles.AnyAsync(v => v.Id == id);
    }
    
}