using System.Dynamic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public class StateManagementService : IStateManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<State> _stateSortHelper;
    private readonly IDataShaper<State> _stateDataShaper;

    public StateManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<State> stateSortHelper, 
        IDataShaper<State> stateDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _stateSortHelper = stateSortHelper;
        _stateDataShaper = stateDataShaper;
    }

    public async Task<(bool isSucceed, string message, StateDto state)> AddState(CreateStateDto createStateDto)
    {
        var state = _mapper.Map<State>(createStateDto);
    
        await _dbContext.States.AddAsync(state);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<StateDto>(state));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<StateDto> states,
            PagingMetadata<State> pagingMetadata)> GetStates(StateParameters parameters)
    {
        var dbStates = _dbContext.States.Include(s => s.Country)
            .Include(s => s.Cities)
            .ThenInclude(c => c.Addresses).AsQueryable();
        
        SearchByAllStateFields(ref dbStates, parameters.Search);
        FilterByStateName(ref dbStates, parameters.Name);
        FilterByCountryId(ref dbStates, parameters.CountryId);

        try
        {
            dbStates = _stateSortHelper.ApplySort(dbStates, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbStates.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbStates, parameters.PageNumber,
            parameters.PageSize);

        var shapedStatesData = _stateDataShaper.ShapeData(dbStates, parameters.Fields);
        var stateDtos = shapedStatesData.ToList().ConvertAll(s => _mapper.Map<StateDto>(s));
        
        return (true, "", stateDtos, pagingMetadata);

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

        PagingMetadata<State> ApplyPaging(ref IQueryable<State> states,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<State>(states,
                pageNumber, pageSize);
            
            states = states
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, StateDto state)> GetState(int id, string? fields)
    {
        var dbState = await _dbContext.States.Where(s => s.Id == id)
            .Include(s => s.Country).Include(s => s.Cities)
            .ThenInclude(c => c.Addresses)
            .FirstOrDefaultAsync();

        if (dbState == null)
        {
            return (false, $"State doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = StateParameters.DefaultFields;
        }
        
        var shapedStateData = _stateDataShaper.ShapeData(dbState, fields);
        var stateDto = _mapper.Map<StateDto>(shapedStateData);

        return (true, "", stateDto);
    }

    public async Task<(bool isSucceed, string message, UpdateStateDto state)> UpdateState(UpdateStateDto updateStateDto)
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
                return (false, $"State with id:{updateStateDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbState = await _dbContext.States.FirstOrDefaultAsync(s => s.Id == state.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateStateDto>(dbState));
    }

    public async Task<(bool isSucceed, string message)> DeleteState(int id)
    {
        var dbState = await _dbContext.States.FirstOrDefaultAsync(s => s.Id == id);
    
        if (dbState == null)
        {
            return (false, $"State with id:{id} doesn't exist");
        }
    
        _dbContext.States.Remove(dbState);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsStateExists(int id)
    {
        return await _dbContext.States.AnyAsync(s => s.Id == id);
    }
}