using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IAddressManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, AddressDto address)> AddAddress(CreateAddressDto createAddressDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> addresses,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetAddresses(AddressParameters parameters);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject address)> GetAddress(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, AddressDto address)> UpdateAddress(UpdateAddressDto updateAddressDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteAddress(int id);
    Task<bool> IsAddressExists(int id);
}