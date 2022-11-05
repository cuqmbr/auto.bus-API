using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public interface IAddressManagementService
{
    Task<(bool isSucceed, string message, AddressDto address)> AddAddress(CreateAddressDto createAddressDto);

    Task<(bool isSucceed, string message, IEnumerable<AddressDto> addresses,
        PagingMetadata<Address> pagingMetadata)> GetAddresses(AddressParameters parameters);
    Task<(bool isSucceed, string message, AddressDto address)> GetAddress(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateAddressDto address)> UpdateAddress(UpdateAddressDto updateAddressDto);
    Task<(bool isSucceed, string message)> DeleteAddress(int id);
    Task<bool> IsAddressExists(int id);
}