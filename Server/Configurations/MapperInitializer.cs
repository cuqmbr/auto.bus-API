using System.Dynamic;
using AutoMapper;
using Server.Models;
using SharedModels.DataTransferObjects;
using Route = Server.Models.Route;

namespace Server.Configurations;

public class MapperInitializer : Profile
{
    public MapperInitializer()
    {
        CreateMap<Country, CountryDto>().ReverseMap();
        CreateMap<Country, CreateCountryDto>().ReverseMap();
        CreateMap<Country, UpdateCountryDto>().ReverseMap();
        CreateMap<Country, InStateCountryDto>().ReverseMap();
        
        CreateMap<State, StateDto>().ReverseMap();
        CreateMap<State, CreateStateDto>().ReverseMap();
        CreateMap<State, UpdateStateDto>().ReverseMap();
        CreateMap<State, InCountryStateDto>().ReverseMap();

        CreateMap<City, CityDto>().ReverseMap();
        CreateMap<City, CreateCityDto>().ReverseMap();

        CreateMap<Address, AddressDto>().ReverseMap();
        CreateMap<Address, CreateAddressDto>().ReverseMap();

        CreateMap<RouteAddress, RouteAddressDto>().ReverseMap();
        CreateMap<RouteAddress, CreateRouteAddressDto>().ReverseMap();

        CreateMap<Route, RouteDto>().ReverseMap();
        CreateMap<Route, CreateRouteDto>().ReverseMap();
    }
}