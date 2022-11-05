using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public class AddressManagementService : IAddressManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<Address> _addressSortHelper;
    private readonly IDataShaper<Address> _addressDataShaper;

    public AddressManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<Address> addressSortHelper, 
        IDataShaper<Address> addressDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _addressSortHelper = addressSortHelper;
        _addressDataShaper = addressDataShaper;
    }

    public async Task<(bool isSucceed, string message, AddressDto address)> AddAddress(CreateAddressDto createAddressDto)
    {
        var address = _mapper.Map<Address>(createAddressDto);
    
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<AddressDto>(address));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<AddressDto> addresses,
            PagingMetadata<Address> pagingMetadata)> GetAddresses(AddressParameters parameters)
    {
        var dbAddresses = _dbContext.Addresses.Include(a => a.City)
            .ThenInclude(c => c.State).ThenInclude(s => s.Country)
            .AsQueryable();
        
        SearchByAllAddressFields(ref dbAddresses, parameters.Search);
        SearchByAddressName(ref dbAddresses, parameters.Name);
        SearchByCityId(ref dbAddresses, parameters.CityId);

        try
        {
            dbAddresses = _addressSortHelper.ApplySort(dbAddresses, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbAddresses.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbAddresses, parameters.PageNumber,
            parameters.PageSize);

        var shapedAddressesData = _addressDataShaper.ShapeData(dbAddresses, parameters.Fields);
        var addressDtos = shapedAddressesData.ToList().ConvertAll(a => _mapper.Map<AddressDto>(a));
        
        return (true, "", addressDtos, pagingMetadata);

        void SearchByAllAddressFields(ref IQueryable<Address> addresses,
            string? search)
        {
            if (!addresses.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            addresses = addresses.Where(a =>
                a.Name.ToLower().Contains(search.ToLower()));
        }
        
        void SearchByCityId(ref IQueryable<Address> addresses,
            int? cityId)
        {
            if (!addresses.Any() || cityId == null)
            {
                return;
            }

            addresses = addresses.Where(a => a.CityId == cityId);
        }
        
        void SearchByAddressName(ref IQueryable<Address> addresses,
            string? addressName)
        {
            if (!addresses.Any() || String.IsNullOrWhiteSpace(addressName))
            {
                return;
            }

            addresses = addresses.Where(a =>
                a.Name.ToLower().Contains(addressName.Trim().ToLower()));
        }

        PagingMetadata<Address> ApplyPaging(ref IQueryable<Address> addresses,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<Address>(addresses,
                pageNumber, pageSize);
            
            addresses = addresses
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, AddressDto address)> GetAddress(int id, string? fields)
    {
        var dbAddress = await _dbContext.Addresses.Where(a => a.Id == id)
            .Include(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)
            .FirstOrDefaultAsync();

        if (dbAddress == null)
        {
            return (false, $"Address doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = AddressParameters.DefaultFields;
        }
        
        var shapedAddressData = _addressDataShaper.ShapeData(dbAddress, fields);
        var addressDto = _mapper.Map<AddressDto>(shapedAddressData);

        return (true, "", addressDto);
    }

    public async Task<(bool isSucceed, string message, UpdateAddressDto address)> UpdateAddress(UpdateAddressDto updateAddressDto)
    {
        var address = _mapper.Map<Address>(updateAddressDto);
        _dbContext.Entry(address).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsAddressExists(updateAddressDto.Id))
            {
                return (false, $"Address with id:{updateAddressDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbAddress = await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == address.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateAddressDto>(dbAddress));
    }

    public async Task<(bool isSucceed, string message)> DeleteAddress(int id)
    {
        var dbAddress = await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == id);
    
        if (dbAddress == null)
        {
            return (false, $"Address with id:{id} doesn't exist");
        }
    
        _dbContext.Addresses.Remove(dbAddress);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsAddressExists(int id)
    {
        return await _dbContext.Addresses.AnyAsync(a => a.Id == id);
    }
}