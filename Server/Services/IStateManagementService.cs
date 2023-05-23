using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IStateManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, StateDto state)> AddState(CreateStateDto createStateDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> states,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetStates(StateParameters parameters);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject state)> GetState(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, StateDto state)> UpdateState(UpdateStateDto updateStateDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteState(int id);
    Task<bool> IsStateExists(int id);
}