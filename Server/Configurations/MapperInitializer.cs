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
        CreateMap<State, InCityStateDto>().ReverseMap();

        CreateMap<City, CityDto>().ReverseMap();
        CreateMap<City, CreateCityDto>().ReverseMap();
        CreateMap<City, UpdateCityDto>().ReverseMap();
        CreateMap<City, InStateCityDto>().ReverseMap();
        CreateMap<City, InAddressCityDto>().ReverseMap();

        CreateMap<Address, AddressDto>().ReverseMap();
        CreateMap<Address, CreateAddressDto>().ReverseMap();
        CreateMap<Address, UpdateAddressDto>().ReverseMap();
        CreateMap<Address, InCityAddressDto>().ReverseMap();

        CreateMap<RouteAddress, RouteAddressDto>().ReverseMap();
        CreateMap<RouteAddress, CreateRouteAddressDto>().ReverseMap();
        CreateMap<RouteAddress, UpdateRouteAddressDto>().ReverseMap();

        CreateMap<Route, RouteDto>().ReverseMap();
        CreateMap<Route, CreateRouteDto>().ReverseMap();
        CreateMap<Route, UpdateRouteDto>().ReverseMap();





        CreateMap<Ticket, TicketDto>().ReverseMap();
        CreateMap<Ticket, CreateTicketDto>().ReverseMap();
        CreateMap<Ticket, UpdateTicketDto>().ReverseMap();

        CreateMap<Review, ReviewDto>().ReverseMap();
        CreateMap<Review, CreateReviewDto>().ReverseMap();
        CreateMap<Review, UpdateReviewDto>().ReverseMap();
        
        CreateMap<Company, CompanyDto>().ReverseMap();
        CreateMap<Company, CreateCompanyDto>().ReverseMap();
        CreateMap<Company, UpdateCompanyDto>().ReverseMap();
        
        CreateMap<Vehicle, VehicleDto>().ReverseMap();
        CreateMap<Vehicle, CreateVehicleDto>().ReverseMap();
        CreateMap<Vehicle, UpdateVehicleDto>().ReverseMap();
        
        CreateMap<VehicleEnrollment, VehicleEnrollmentDto>().ReverseMap();
        CreateMap<VehicleEnrollment, CreateVehicleEnrollmentDto>().ReverseMap();
        CreateMap<VehicleEnrollment, UpdateVehicleEnrollmentDto>().ReverseMap();


        // CreateMap<User, UserDto>().ReverseMap();
        // CreateMap<User, CreateUserDto>().ReverseMap();
        // CreateMap<User, UpdateUserDto>().ReverseMap();
    }
}