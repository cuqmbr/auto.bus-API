using System.Dynamic;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Server.Configurations;
using Server.Data;
using Server.Helpers;
using Server.Models;
using Server.Services;
using SharedModels.DataTransferObjects;
using Utils;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.

services.AddControllers().AddNewtonsoftJson(options => {
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
});
services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin()
        .AllowAnyHeader().AllowAnyMethod());
});

services.AddIdentityCore<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// Configuration from AppSettings
services.Configure<SmtpCredentials>(configuration.GetSection("SmtpCredentials"));
services.Configure<Jwt>(configuration.GetSection("Jwt"));

// Adding Authentication - JWT
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // options.RequireHttpsMetadata = false;
        // options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });

services.AddAuthorization(options => {
    // options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
 
    // Policies for accessing endpoints on a top level based on user role
    options.AddPolicy(Identity.Roles.User + "Access", policy => 
        policy.RequireRole(Identity.Roles.User.ToString()));
    options.AddPolicy(Identity.Roles.Driver + "Access", policy => 
        policy.RequireRole(Identity.Roles.Driver.ToString(), Identity.Roles.Company.ToString(),
            Identity.Roles.Administrator.ToString()));
    options.AddPolicy(Identity.Roles.Company + "Access", policy => 
        policy.RequireRole(Identity.Roles.Company.ToString(), Identity.Roles.Administrator.ToString()));
    options.AddPolicy(Identity.Roles.Administrator + "Access", policy => 
        policy.RequireRole(Identity.Roles.Administrator.ToString()));
});

services.AddAutoMapper(typeof(MapperInitializer));

services.AddScoped<IEmailSenderService, EmailSenderService>();
services.AddScoped<IAuthenticationService, AuthenticationService>();

services.AddScoped<ICountryManagementService, CountryManagementService>();
services.AddScoped<IStateManagementService, StateManagementService>();
services.AddScoped<ICityManagementService, CityManagementService>();
services.AddScoped<IAddressManagementService, AddressManagementService>();
services.AddScoped<ITicketManagementService, TicketManagementService>();
services.AddScoped<ITicketGroupManagementService, TicketGroupManagementService>();
services.AddScoped<IReviewManagementService, ReviewManagementService>();
services.AddScoped<ICompanyManagementService, CompanyManagementService>();
services.AddScoped<IVehicleManagementService, VehicleManagementService>();
services.AddScoped<IVehicleEnrollmentManagementService, VehicleEnrollmentManagementService>();
services.AddScoped<IRouteManagementService, RouteManagementService>();
services.AddScoped<IRouteAddressManagementService, RouteAddressManagementService>();
services.AddScoped<IUserManagementService, UserManagementService>();
services.AddScoped<IDriverManagementService, DriverManagementService>();

services.AddScoped<IDataShaper<CountryDto>, DataShaper<CountryDto>>();
services.AddScoped<IDataShaper<StateDto>, DataShaper<StateDto>>();
services.AddScoped<IDataShaper<CityDto>, DataShaper<CityDto>>();
services.AddScoped<IDataShaper<AddressDto>, DataShaper<AddressDto>>();
services.AddScoped<IDataShaper<TicketDto>, DataShaper<TicketDto>>();
services.AddScoped<IDataShaper<TicketGroupDto>, DataShaper<TicketGroupDto>>();
services.AddScoped<IDataShaper<TicketGroupWithTicketsDto>, DataShaper<TicketGroupWithTicketsDto>>();
services.AddScoped<IDataShaper<ReviewDto>, DataShaper<ReviewDto>>();
services.AddScoped<IDataShaper<CompanyDto>, DataShaper<CompanyDto>>();
services.AddScoped<IDataShaper<VehicleDto>, DataShaper<VehicleDto>>();
services.AddScoped<IDataShaper<VehicleEnrollmentDto>, DataShaper<VehicleEnrollmentDto>>();
services.AddScoped<IDataShaper<VehicleEnrollmentWithDetailsDto>, DataShaper<VehicleEnrollmentWithDetailsDto>>();
services.AddScoped<IDataShaper<RouteDto>, DataShaper<RouteDto>>();
services.AddScoped<IDataShaper<RouteWithAddressesDto>, DataShaper<RouteWithAddressesDto>>();
services.AddScoped<IDataShaper<RouteAddressDto>, DataShaper<RouteAddressDto>>();
services.AddScoped<IDataShaper<UserDto>, DataShaper<UserDto>>();
services.AddScoped<IDataShaper<DriverDto>, DataShaper<DriverDto>>();
services.AddScoped<IDataShaper<ExpandoObject>, DataShaper<ExpandoObject>>();

services.AddScoped<ISortHelper<ExpandoObject>, SortHelper<ExpandoObject>>();
services.AddScoped<IPager<ExpandoObject>, Pager<ExpandoObject>>();

services.AddScoped<AutomationService>();
services.AddScoped<IReportService, ReportService>();
services.AddScoped<IStatisticsService, StatisticsService>();

// Adding DB Context with PostgreSQL
var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Data seeding
using var scope = app.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;
await SeedData.Initialize(serviceProvider);

// Configure the HTTP request pipeline.
if (Convert.ToBoolean(configuration["UseApiExplorer"]))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/*
app.UseHttpsRedirection();
*/

app.UseAuthentication();
app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();