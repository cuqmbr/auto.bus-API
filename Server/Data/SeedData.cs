using Microsoft.AspNetCore.Identity;
using Server.Models;
using Utils;
using Route = Server.Models.Route;

namespace Server.Data;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = (UserManager<User>)serviceProvider.GetService(typeof(UserManager<User>))!;
        userManager.UserValidators.Clear();
        
        var roleManager = (RoleManager<IdentityRole>)serviceProvider.GetService(typeof(RoleManager<IdentityRole>))!;
        var dbContext = (ApplicationDbContext)serviceProvider.GetService(typeof(ApplicationDbContext))!;

        // Seed Roles
        foreach (var role in Enum.GetValues(typeof(Identity.Roles)))
        {
            await roleManager.CreateAsync(new IdentityRole(role.ToString()));
        }

        User? companyUser;
        User driverUser;
        if (!dbContext.Users.Any())
        {
            // Seed Administrator user
            
            var adminUser = new User
            {
                FirstName = "user", LastName = "user", Patronymic = "user",
                Email = "admin@autobus.com",
                EmailConfirmed = true,
            };
            
            await userManager.CreateAsync(adminUser, Identity.DefaultPassword);
            await userManager.AddToRoleAsync(adminUser, Identity.Roles.Administrator.ToString());
            
            // Seed Company user
            
            companyUser = new User
            {
                FirstName = "user", LastName = "user", Patronymic = "user",
                Email = "company@autobus.com",
                EmailConfirmed = true
            };
            
            await userManager.CreateAsync(companyUser, Identity.DefaultPassword);
            await userManager.AddToRoleAsync(companyUser, Identity.Roles.Company.ToString());
            
            // Seed Driver user
            
            driverUser = new User
            {
                FirstName = "user", LastName = "user", Patronymic = "user",
                Email = "driver@autobus.com",
                EmailConfirmed = true
            };
            
            await userManager.CreateAsync(driverUser, Identity.DefaultPassword);
            await userManager.AddToRoleAsync(driverUser, Identity.Roles.Driver.ToString());
            
            // Seed Default user
            
            var defaultUser = new User
            {
                FirstName = "user", LastName = "user", Patronymic = "user",
                Email = "user@autobus.com",
                EmailConfirmed = true
            };
            
            await userManager.CreateAsync(defaultUser, Identity.DefaultPassword);
            await userManager.AddToRoleAsync(defaultUser, Identity.Roles.User.ToString());
        }

        companyUser = await userManager.FindByEmailAsync("company@autobus.com");
        driverUser = await userManager.FindByEmailAsync("driver@autobus.com");

        if (!dbContext.Companies.Any())
        {
            // Seed County - State - City - Address relations

            await dbContext.Countries.AddRangeAsync(new Country { Name = "Ukraine", Code = "UA" });
            await dbContext.SaveChangesAsync();
            
            await dbContext.States.AddRangeAsync(new State { CountryId = 1, Name = "Kharkiv Oblast" });
            await dbContext.SaveChangesAsync();
            
            await dbContext.Cities.AddRangeAsync(new City { StateId = 1, Name = "Osykovyi Hai" },
                new City { StateId = 1, Name = "Malynivka" },
                new City { StateId = 1, Name = "Chuhuiv" },
                new City { StateId = 1, Name = "Kam'yana Yaruha" },
                new City { StateId = 1, Name = "Rohan'" },
                new City { StateId = 1, Name = "Kharkiv" },
                new City { StateId = 1, Name = "Korobochkyne" },
                new City { StateId = 1, Name = "Kochetok" },
                new City { StateId = 1, Name = "Kytsivka" });
            await dbContext.SaveChangesAsync();

            await dbContext.Addresses.AddRangeAsync(new Address { CityId = 1, Name = "station near E40 road", Latitude = 36.778044, Longitude = 49.777235 },
                new Address { CityId = 2, Name = "v. Malynivka", Latitude = 36.732271, Longitude = 49.813638 },
                new Address { CityId = 3, Name = "Chuhuyivsʹka Avtostantsiya, Vulytsya Kharkivsʹka, 133", Latitude = 36.687534, Longitude = 49.838337 },
                new Address { CityId = 4, Name = "Kulynychi", Latitude = 36.595877, Longitude = 49.880585 },
                new Address { CityId = 5, Name = "v. Rohan", Latitude = 36.495910, Longitude = 49.907080 },
                new Address { CityId = 6, Name = "Kharkiv Parking Near Mcdonalds", Latitude = 36.207202, Longitude = 49.987560 },
                new Address { CityId = 7, Name = "Ukrposhta 63540, Chuhuivs'ka St, 2Б", Latitude = 36.812743, Longitude = 49.776519 },
                new Address { CityId = 8, Name = "Pivnyk", Latitude = 36.712386, Longitude = 49.867814 },
                new Address { CityId = 9, Name = "v. Kytsivka", Latitude =  36.819010, Longitude = 49.869401 });
            await dbContext.SaveChangesAsync();

            // Seed Route - RouteAddresses relations
            
            await dbContext.Routes.AddRangeAsync(
                new Route
                {
                    Type = "default",
                    RouteAddresses = new List<RouteAddress>
                    {
                        new RouteAddress { Order = 1, AddressId = 1},
                        new RouteAddress { Order = 2, AddressId = 2},
                        new RouteAddress { Order = 3, AddressId = 3},
                        new RouteAddress { Order = 4, AddressId = 4},
                        new RouteAddress { Order = 5, AddressId = 5},
                        new RouteAddress { Order = 6, AddressId = 6},
                    }
                },
                new Route
                {
                    Type = "default",
                    RouteAddresses = new List<RouteAddress>
                    {
                        new RouteAddress { Order = 1, AddressId = 7},
                        new RouteAddress { Order = 2, AddressId = 2},
                        new RouteAddress { Order = 3, AddressId = 3},
                        new RouteAddress { Order = 4, AddressId = 8},
                        new RouteAddress { Order = 5, AddressId = 9},
                    }
                });
            await dbContext.SaveChangesAsync();

            
            await dbContext.Companies.AddAsync(
                new Company
                {
                    OwnerId = companyUser.Id,
                    Name = "Default Company",
                    CompanyDrivers = new []
                    {
                        new CompanyDriver
                        {
                            DriverId = driverUser.Id
                        }
                    }
                });
            await dbContext.SaveChangesAsync();

            await dbContext.Vehicles.AddRangeAsync(
                new Vehicle
                {
                    Number = "AC8376BH",
                    Type = "Cruiser",
                    Capacity = 50,
                    CompanyId = 1,
                    HasClimateControl = true,
                    HasWiFi = true,
                    HasWC = true,
                    HasStewardess = true,
                    HasTV = false,
                    HasOutlet = true,
                    HasBelts = true,
                },
                new Vehicle
                {
                    Number = "HA8934MK",
                    Type = "Lon Rider",
                    Capacity = 25,
                    CompanyId = 1,
                    HasClimateControl = false,
                    HasWiFi = false,
                    HasWC = false,
                    HasStewardess = false,
                    HasTV = false,
                    HasOutlet = false,
                    HasBelts = false,
                });
            await dbContext.SaveChangesAsync();

            await dbContext.VehicleEnrollments.AddRangeAsync(
                new VehicleEnrollment
                {
                    RouteId = 1,
                    VehicleId = 1,
                    DepartureDateTimeUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 07, 45, 00, DateTimeKind.Utc),
                    RouteAddressDetails = new List<RouteAddressDetails>
                    {
                        new RouteAddressDetails
                    {
                            RouteAddressId = 1,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(20),
                            CostToNextCity = 25
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 2,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(5),
                            WaitTimeSpan = TimeSpan.FromMinutes(15),
                            CostToNextCity = 20
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 3,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(5),
                            WaitTimeSpan = TimeSpan.FromMinutes(25),
                            CostToNextCity = 30
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 4,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(5),
                            WaitTimeSpan = TimeSpan.FromMinutes(20),
                            CostToNextCity = 35
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 5,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(5),
                            WaitTimeSpan = TimeSpan.FromMinutes(30),
                            CostToNextCity = 40
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 6,
                        }
                    }
                },
                new VehicleEnrollment
                {
                    RouteId = 2,
                    VehicleId = 2,
                    DepartureDateTimeUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 08, 00, 00, DateTimeKind.Utc),
                    RouteAddressDetails = new List<RouteAddressDetails>
                    {
                        new RouteAddressDetails
                        {
                            RouteAddressId = 7,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(20),
                            CostToNextCity = 25
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 8,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(5),
                            WaitTimeSpan = TimeSpan.FromMinutes(15),
                            CostToNextCity = 20
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 9,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(5),
                            WaitTimeSpan = TimeSpan.FromMinutes(25),
                            CostToNextCity = 30
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 10,
                            TimeSpanToNextCity = TimeSpan.FromMinutes(5),
                            WaitTimeSpan = TimeSpan.FromMinutes(20),
                            CostToNextCity = 35
                        },
                        new RouteAddressDetails
                        {
                            RouteAddressId = 11,
                        }
                    }
                });

            await dbContext.SaveChangesAsync();
            
            await dbContext.TicketGroups.AddAsync(
                new TicketGroup
                {
                    UserId = companyUser.Id,
                    Tickets = new List<Ticket>
                    {
                        new Ticket
                        {
                            VehicleEnrollmentId = 1,
                            PurchaseDateTimeUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 08, 00, 00, DateTimeKind.Utc).AddDays(-1),
                            FirstRouteAddressId = 1,
                            LastRouteAddressId = 2
                        },
                        new Ticket
                        {
                            VehicleEnrollmentId = 2,
                            PurchaseDateTimeUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 08, 00, 00, DateTimeKind.Utc).AddDays(-1),
                            FirstRouteAddressId = 2,
                            LastRouteAddressId = 9
                        }
                    }
                });

            dbContext.Reviews.AddRangeAsync(
                new Review
                {
                    UserId = companyUser.Id,
                    VehicleEnrollmentId = 1,
                    Rating = 85,
                    Comment = "Good overall",
                    PostDateTimeUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 08, 00, 00, DateTimeKind.Utc).AddDays(1),
                }, new Review
                {
                    UserId = companyUser.Id,
                    VehicleEnrollmentId = 2,
                    Rating = 100,
                    Comment = "Amazing",
                    PostDateTimeUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 08, 00, 00, DateTimeKind.Utc).AddDays(1),
                });
            
            await dbContext.SaveChangesAsync();
        }
    }
}