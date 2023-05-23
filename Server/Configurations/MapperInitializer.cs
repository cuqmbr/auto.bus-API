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

        CreateMap<RouteAddress, RouteAddressDto>().ReverseMap();
        CreateMap<RouteAddress, CreateRouteAddressDto>().ReverseMap();
        CreateMap<RouteAddress, UpdateRouteAddressDto>().ReverseMap();
        CreateMap<RouteAddress, CreateRouteAddressWithAddressDto>().ReverseMap();
        CreateMap<RouteAddress, RouteAddressWithAddressDto>().ReverseMap();

        CreateMap<Route, RouteDto>().ReverseMap();
        CreateMap<Route, CreateRouteDto>().ReverseMap();
        CreateMap<Route, UpdateRouteDto>().ReverseMap();
        CreateMap<Route, CreateRouteWithAddressesDto>().ReverseMap();
        CreateMap<Route, RouteWithAddressesDto>().ReverseMap();

        CreateMap<Ticket, TicketDto>().IncludeMembers(t => t.TicketGroup).ReverseMap();
        CreateMap<TicketGroup, TicketDto>();
        CreateMap<Ticket, CreateTicketDto>().ReverseMap();
        CreateMap<Ticket, UpdateTicketDto>().ReverseMap();
        CreateMap<Ticket, CreateInTicketGroupTicketDto>().ReverseMap();
        CreateMap<Ticket, InTicketGroupTicketDto>();
        CreateMap<Ticket, InVehicleEnrollmentTicketDto>();
        
        CreateMap<TicketGroup, TicketGroupDto>().ReverseMap();
        CreateMap<TicketGroup, CreateTicketGroupDto>().ReverseMap();
        CreateMap<TicketGroup, UpdateTicketGroupDto>().ReverseMap();
        CreateMap<TicketGroup, CreateTicketGroupWithTicketsDto>().ReverseMap();
        CreateMap<TicketGroup, TicketGroupWithTicketsDto>().ReverseMap();

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
        CreateMap<VehicleEnrollment, VehicleEnrollmentWithDetailsDto>().ReverseMap();
        CreateMap<VehicleEnrollment, CreateVehicleEnrollmentWithDetailsDto>().ReverseMap();
        CreateMap<VehicleEnrollment, InReviewVehicleEnrollmentDto>();

        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UpdateUserDto>().ReverseMap();
        CreateMap<User, StrippedUserDto>().ReverseMap();
        
        CreateMap<User, DriverDto>().ForMember(d => d.CompanyId, o => o.MapFrom(s => s.Employer.CompanyId));
        CreateMap<UserDto, DriverDto>().ReverseMap();
        CreateMap<CreateUserDto, CreateDriverDto>().ReverseMap();
        CreateMap<CreateDriverDto, UpdateDriverDto>().ReverseMap();
        
        CreateMap<RouteAddressDetails, RouteAddressDetailsDto>().ReverseMap();
        CreateMap<RouteAddressDetails, CreateRouteAddressDetailsDto>().ReverseMap();
        CreateMap<RouteAddressDetails, UpdateRouteAddressDetailsDto>().ReverseMap();
        CreateMap<RouteAddressDetails, RouteAddressDetailsInVehicleEnrollmentDto>().ReverseMap();
        CreateMap<RouteAddressDetails, CreateRouteAddressDetailsInVehicleEnrollmentDto>().ReverseMap();
    }
}
