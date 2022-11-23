using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IStateManagementService
{
    Task<(bool isSucceed, string message, StateDto state)> AddState(CreateStateDto createStateDto);
    Task<(bool isSucceed, string message, IEnumerable<StateDto> states,
        PagingMetadata<State> pagingMetadata)> GetStates(StateParameters parameters);
    Task<(bool isSucceed, string message, StateDto state)> GetState(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateStateDto state)> UpdateState(UpdateStateDto updateStateDto);
    Task<(bool isSucceed, string message)> DeleteState(int id);
    Task<bool> IsStateExists(int id);
}