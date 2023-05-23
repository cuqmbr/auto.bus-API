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

namespace Server.Services;

public class StateManagementService : IStateManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _stateSortHelper;
    private readonly IDataShaper<StateDto> _stateDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public StateManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> stateSortHelper, 
        IDataShaper<StateDto> stateDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _stateSortHelper = stateSortHelper;
        _stateDataShaper = stateDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, StateDto state)> AddState(CreateStateDto createStateDto)
    {
        var state = _mapper.Map<State>(createStateDto);
    
        await _dbContext.States.AddAsync(state);
        await _dbContext.SaveChangesAsync();

        state = await _dbContext.States.Include(s => s.Country)
            .FirstAsync(s => s.Id == state.Id);
    
        return (true, null, _mapper.Map<StateDto>(state));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> states,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetStates(StateParameters parameters)
    {
        var dbStates = _dbContext.States.Include(s => s.Country)
            .Include(s => s.Cities)
            .ThenInclude(c => c.Addresses).AsQueryable();

        SearchByAllStateFields(ref dbStates, parameters.Search);
        FilterByStateName(ref dbStates, parameters.Name);
        FilterByCountryId(ref dbStates, parameters.CountryId);

        var stateDtos = _mapper.ProjectTo<StateDto>(dbStates);
        var shapedData = _stateDataShaper.ShapeData(stateDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _stateSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

        void SearchByAllStateFields(ref IQueryable<State> states,
            string? search)
        {
            if (!states.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            states = states.Where(s =>
                s.Name.ToLower().Contains(search.ToLower()));
        }
        
        void FilterByCountryId(ref IQueryable<State> states,
            int? countryId)
        {
            if (!states.Any() || countryId == null)
            {
                return;
            }

            states = states.Where(s => s.CountryId == countryId);
        }
        
        void FilterByStateName(ref IQueryable<State> states,
            string? stateName)
        {
            if (!states.Any() || String.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            states = states.Where(s =>
                s.Name.ToLower().Contains(stateName.Trim().ToLower()));
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject state)> GetState(int id, string? fields)
    {
        if (!await IsStateExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbState = await _dbContext.States.Where(s => s.Id == id)
            .Include(s => s.Country).Include(s => s.Cities)
            .ThenInclude(c => c.Addresses)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = StateParameters.DefaultFields;
        }
        
        var stateDto = _mapper.Map<StateDto>(dbState);
        var shapedData = _stateDataShaper.ShapeData(stateDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, StateDto state)> UpdateState(UpdateStateDto updateStateDto)
    {
        var state = _mapper.Map<State>(updateStateDto);
        _dbContext.Entry(state).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsStateExists(updateStateDto.Id))
            {
                return (false, new NotFoundResult(), null!);
            }
        }

        var dbState = await _dbContext.States.FirstAsync(s => s.Id == state.Id);
        
        return (true, null, _mapper.Map<StateDto>(dbState));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteState(int id)
    {
        var dbState = await _dbContext.States.FirstOrDefaultAsync(s => s.Id == id);
    
        if (dbState == null)
        {
            return (false, new NotFoundResult());
        }
    
        _dbContext.States.Remove(dbState);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsStateExists(int id)
    {
        return await _dbContext.States.AnyAsync(s => s.Id == id);
    }
}