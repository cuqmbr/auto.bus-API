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

public class AddressManagementService : IAddressManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _addressSortHelper;
    private readonly IDataShaper<AddressDto> _addressDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public AddressManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> addressSortHelper, 
        IDataShaper<AddressDto> addressDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _addressSortHelper = addressSortHelper;
        _addressDataShaper = addressDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, AddressDto address)> AddAddress(CreateAddressDto createAddressDto)
    {
        var address = _mapper.Map<Address>(createAddressDto);
    
        await _dbContext.Addresses.AddAsync(address);
        await _dbContext.SaveChangesAsync();

        address = await _dbContext.Addresses.Include(a => a.City)
            .ThenInclude(c => c.State).ThenInclude(s => s.Country)
            .FirstAsync(a => a.Id == address.Id);
    
        return (true, null, _mapper.Map<AddressDto>(address));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> addresses,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetAddresses(AddressParameters parameters)
    {
        var dbAddresses = _dbContext.Addresses.Include(a => a.City)
            .ThenInclude(c => c.State).ThenInclude(s => s.Country)
            .AsQueryable();
        
        SearchByAllAddressFields(ref dbAddresses, parameters.Search);
        FilterByAddressName(ref dbAddresses, parameters.Name);
        FilterByCityId(ref dbAddresses, parameters.CityId);


        var addressDtos = dbAddresses.ToList().ConvertAll(a => _mapper.Map<AddressDto>(a));
        var shapedData = _addressDataShaper.ShapeData(addressDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _addressSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null, null)!;
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

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
        
        void FilterByCityId(ref IQueryable<Address> addresses,
            int? cityId)
        {
            if (!addresses.Any() || cityId == null)
            {
                return;
            }

            addresses = addresses.Where(a => a.CityId == cityId);
        }
        
        void FilterByAddressName(ref IQueryable<Address> addresses,
            string? addressName)
        {
            if (!addresses.Any() || String.IsNullOrWhiteSpace(addressName))
            {
                return;
            }

            addresses = addresses.Where(a =>
                a.Name.ToLower().Contains(addressName.Trim().ToLower()));
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject address)> GetAddress(int id, string? fields)
    {
        if (!await IsAddressExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbAddress = await _dbContext.Addresses.Where(a => a.Id == id)
            .Include(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = AddressParameters.DefaultFields;
        }
        
        var addressDto = _mapper.Map<AddressDto>(dbAddress);
        var shapedData = _addressDataShaper.ShapeData(addressDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, AddressDto address)> UpdateAddress(UpdateAddressDto updateAddressDto)
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
                return (false, new NotFoundResult(), null!);
            }
        }

        var dbAddress = await _dbContext.Addresses.FirstAsync(a => a.Id == address.Id);
        
        return (true, null, _mapper.Map<AddressDto>(dbAddress));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteAddress(int id)
    {
        var dbAddress = await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == id);

        if (dbAddress == null)
        {
            return (false, new NotFoundResult());
        }
        
        _dbContext.Addresses.Remove(dbAddress);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsAddressExists(int id)
    {
        return await _dbContext.Addresses.AnyAsync(a => a.Id == id);
    }
}