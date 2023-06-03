using AutoMapper;
using Server.Models;
using SharedModels.DataTransferObjects.Model;
using Route = Server.Models.Route;

namespace Server.Configurations;

public class MapperInitializer : Profile
{
    public MapperInitializer()
    {
        RecognizePostfixes("Utc");
        RecognizeDestinationPostfixes("Utc");
        
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
        CreateMap<Address, CreateAddressInRouteAddress>().ReverseMap();
        CreateMap<Address, AddressInRouteAddress>().ReverseMap();
        CreateMap<Address, InTicketAddress>().ReverseMap();

        CreateMap<RouteAddress, RouteAddressDto>()
            .ForMember(d => d.AddressName, opt => opt.MapFrom(src => src.Address.Name))
            .ForMember(d => d.CityName, opt => opt.MapFrom(src => src.Address.City.Name))
            .ForMember(d => d.StateName, opt => opt.MapFrom(src => src.Address.City.State.Name))
            .ForMember(d => d.CountryName, opt => opt.MapFrom(src => src.Address.City.State.Country.Name))
            .ForMember(d => d.FullName, opt => opt.MapFrom(src => src.Address.GetFullName()))
            .ForMember(d => d.Latitude, opt => opt.MapFrom(src => src.Address.Latitude))
            .ForMember(d => d.Longitude, opt => opt.MapFrom(src => src.Address.Longitude));
        CreateMap<RouteAddressDto, RouteAddress>();
        CreateMap<RouteAddress, CreateRouteAddressDto>().ReverseMap();
        CreateMap<RouteAddress, UpdateRouteAddressDto>().ReverseMap();

        CreateMap<Route, RouteDto>().ReverseMap();
        CreateMap<Route, CreateRouteDto>().ReverseMap();
        CreateMap<Route, UpdateRouteDto>().ReverseMap();

        CreateMap<Ticket, TicketDto>()
            .ForMember(d => d.Addresses,
                opt => opt.MapFrom(src => src.VehicleEnrollment.Route.RouteAddresses.Select(ra => ra.Address)));
        
        CreateMap<TicketGroup, TicketGroupDto>()
            .ForMember(d => d.DepartureAddressName,
                opt => opt.MapFrom(src => src.Tickets.First().GetDepartureAddress().Name))
            .ForMember(d => d.DepartureCityName,
                opt => opt.MapFrom(src => src.Tickets.First().GetDepartureAddress().City.Name))
            .ForMember(d => d.DepartureStateName,
                opt => opt.MapFrom(src => src.Tickets.First().GetDepartureAddress().City.State.Name))
            .ForMember(d => d.DepartureCountryName,
                opt => opt.MapFrom(src => src.Tickets.First().GetDepartureAddress().City.State.Country.Name))
            .ForMember(d => d.DepartureFullName,
                opt => opt.MapFrom(src => src.Tickets.First().GetDepartureAddress().GetFullName()))
            .ForMember(d => d.DepartureDateTime,
                opt => opt.MapFrom(src => src.Tickets.First().GetDepartureTime()))
            .ForMember(d => d.ArrivalAddressName,
                opt => opt.MapFrom(src => src.Tickets.Last().GetArrivalAddress().Name))
            .ForMember(d => d.ArrivalCityName,
                opt => opt.MapFrom(src => src.Tickets.Last().GetArrivalAddress().City.Name))
            .ForMember(d => d.ArrivalStateName,
                opt => opt.MapFrom(src => src.Tickets.Last().GetArrivalAddress().City.State.Name))
            .ForMember(d => d.ArrivalCountryName,
                opt => opt.MapFrom(src => src.Tickets.Last().GetArrivalAddress().City.State.Country.Name))
            .ForMember(d => d.ArrivalFullName,
                opt =>opt.MapFrom(src => src.Tickets.Last().GetArrivalAddress().GetFullName()))
            .ForMember(d => d.ArrivalDateTime, opt => opt.MapFrom(src => src.Tickets.Last().GetArrivalTime()));

        CreateMap<Review, ReviewDto>().ReverseMap();
        CreateMap<Review, CreateReviewDto>().ReverseMap();
        CreateMap<Review, UpdateReviewDto>().ReverseMap();
        CreateMap<Review, InVehicleEnrollmentReviewDto>();
        
        CreateMap<Company, CompanyDto>().ReverseMap();
        CreateMap<Company, CreateCompanyDto>().ReverseMap();
        CreateMap<Company, UpdateCompanyDto>().ReverseMap();
        CreateMap<Company, InVehicleCompanyDto>();
        
        CreateMap<Vehicle, VehicleDto>().ReverseMap();
        CreateMap<Vehicle, CreateVehicleDto>().ReverseMap();
        CreateMap<Vehicle, UpdateVehicleDto>().ReverseMap();
        CreateMap<Vehicle, InVehicleEnrollmentVehicleDto>();
        
        CreateMap<VehicleEnrollment, VehicleEnrollmentDto>().ReverseMap();
        CreateMap<VehicleEnrollment, CreateVehicleEnrollmentDto>().ReverseMap();
        CreateMap<VehicleEnrollment, UpdateVehicleEnrollmentDto>().ReverseMap();
        CreateMap<VehicleEnrollment, InReviewVehicleEnrollmentDto>();

        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UpdateUserDto>().ReverseMap();
        CreateMap<User, StrippedUserDto>().ReverseMap();
        
        CreateMap<User, DriverDto>().ForMember(d => d.CompanyId, o => o.MapFrom(s => s.Employer.CompanyId));
        
        CreateMap<RouteAddressDetails, RouteAddressDetailsInVehicleEnrollmentDto>().ReverseMap();
        CreateMap<RouteAddressDetails, CreateRouteAddressDetailsInVehicleEnrollmentDto>().ReverseMap();
    }
}
