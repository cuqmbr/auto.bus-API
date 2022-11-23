using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public class VehicleManagementService : IVehicleManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<Vehicle> _vehicleSortHelper;
    private readonly IDataShaper<Vehicle> _vehicleDataShaper;

    public VehicleManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<Vehicle> vehicleSortHelper, 
        IDataShaper<Vehicle> vehicleDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _vehicleSortHelper = vehicleSortHelper;
        _vehicleDataShaper = vehicleDataShaper;
    }

    public async Task<(bool isSucceed, string message, VehicleDto vehicle)> AddVehicle(CreateVehicleDto createVehicleDto)
    {
        var vehicle = _mapper.Map<Vehicle>(createVehicleDto);
    
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<VehicleDto>(vehicle));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<VehicleDto> vehicles,
            PagingMetadata<Vehicle> pagingMetadata)> GetVehicles(VehicleParameters parameters)
    {
        var dbVehicles = _dbContext.Vehicles
            .AsQueryable();

        bool a = dbVehicles.Any();
        
        SearchByAllVehicleFields(ref dbVehicles, parameters.Search);
        a = dbVehicles.Any();
        FilterByVehicleNumber(ref dbVehicles, parameters.Number);
        a = dbVehicles.Any();
        FilterByVehicleType(ref dbVehicles, parameters.Type);
        a = dbVehicles.Any();
        FilterByVehicleCapacity(ref dbVehicles, parameters.FromCapacity,
            parameters.ToCapacity);
        a = dbVehicles.Any();
        FilterByVehicleClimateControlAvailability(ref dbVehicles, parameters.HasClimateControl);
        a = dbVehicles.Any();
        FilterByVehicleWiFiAvailability(ref dbVehicles, parameters.HasWiFi);
        a = dbVehicles.Any();
        FilterByVehicleWCAvailability(ref dbVehicles, parameters.HasWC);
        a = dbVehicles.Any();
        FilterByStewardessAvailability(ref dbVehicles, parameters.HasStewardess);
        a = dbVehicles.Any();
        FilterByVehicleTVAvailability(ref dbVehicles, parameters.HasTV);
        a = dbVehicles.Any();
        FilterByVehicleOutletAvailability(ref dbVehicles, parameters.HasOutlet);
        a = dbVehicles.Any();
        FilterByVehicleBeltsAvailability(ref dbVehicles, parameters.HasBelts);
        a = dbVehicles.Any();
        FilterByVehicleCompanyId(ref dbVehicles, parameters.CompanyId);
        a = dbVehicles.Any();
        
        try
        {
            dbVehicles = _vehicleSortHelper.ApplySort(dbVehicles, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbVehicles.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbVehicles, parameters.PageNumber,
            parameters.PageSize);

        var shapedVehiclesData = _vehicleDataShaper.ShapeData(dbVehicles, parameters.Fields);
        var vehicleDtos = shapedVehiclesData.ToList().ConvertAll(v => _mapper.Map<VehicleDto>(v));
        
        return (true, "", vehicleDtos, pagingMetadata);

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

        PagingMetadata<Vehicle> ApplyPaging(ref IQueryable<Vehicle> vehicles,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<Vehicle>(vehicles,
                pageNumber, pageSize);
            
            vehicles = vehicles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, VehicleDto vehicle)> GetVehicle(int id, string? fields)
    {
        var dbVehicle = await _dbContext.Vehicles.Where(v => v.Id == id)
            .FirstOrDefaultAsync();

        if (dbVehicle == null)
        {
            return (false, $"Vehicle doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = VehicleParameters.DefaultFields;
        }
        
        var shapedVehicleData = _vehicleDataShaper.ShapeData(dbVehicle, fields);
        var vehicleDto = _mapper.Map<VehicleDto>(shapedVehicleData);

        return (true, "", vehicleDto);
    }

    public async Task<(bool isSucceed, string message, UpdateVehicleDto vehicle)> UpdateVehicle(UpdateVehicleDto updateVehicleDto)
    {
        var vehicle = _mapper.Map<Vehicle>(updateVehicleDto);
        _dbContext.Entry(vehicle).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsVehicleExists(updateVehicleDto.Id))
            {
                return (false, $"Vehicle with id:{updateVehicleDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbVehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicle.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateVehicleDto>(dbVehicle));
    }

    public async Task<(bool isSucceed, string message)> DeleteVehicle(int id)
    {
        var dbVehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
    
        if (dbVehicle == null)
        {
            return (false, $"Vehicle with id:{id} doesn't exist");
        }
    
        _dbContext.Vehicles.Remove(dbVehicle);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsVehicleExists(int id)
    {
        return await _dbContext.Vehicles.AnyAsync(v => v.Id == id);
    }
}