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
using Server.Constants;
using Server.Data;
using Server.Helpers;
using Server.Models;
using Server.Services;
using SharedModels.DataTransferObjects;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options => {
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
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

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin()
        .AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddIdentityCore<User>(options => {
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 7;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567889-_.";
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

// Configuration from AppSettings
builder.Services.Configure<Jwt>(builder.Configuration.GetSection("Jwt"));
// Adding Authentication - JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // options.RequireHttpsMetadata = false;
        // options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options => {
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

builder.Services.AddAutoMapper(typeof(MapperInitializer));

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddScoped<ICountryManagementService, CountryManagementService>();
builder.Services.AddScoped<IStateManagementService, StateManagementService>();
builder.Services.AddScoped<ICityManagementService, CityManagementService>();
builder.Services.AddScoped<IAddressManagementService, AddressManagementService>();
builder.Services.AddScoped<ITicketManagementService, TicketManagementService>();
builder.Services.AddScoped<ITicketGroupManagementService, TicketGroupManagementService>();
builder.Services.AddScoped<IReviewManagementService, ReviewManagementService>();
builder.Services.AddScoped<ICompanyManagementService, CompanyManagementService>();
builder.Services.AddScoped<IVehicleManagementService, VehicleManagementService>();
builder.Services.AddScoped<IVehicleEnrollmentManagementService, VehicleEnrollmentManagementService>();
builder.Services.AddScoped<IRouteManagementService, RouteManagementService>();
builder.Services.AddScoped<IRouteAddressManagementService, RouteAddressManagementService>();

builder.Services.AddScoped<ISortHelper<ExpandoObject>, SortHelper<ExpandoObject>>();

builder.Services.AddScoped<IDataShaper<CountryDto>, DataShaper<CountryDto>>();
builder.Services.AddScoped<IDataShaper<StateDto>, DataShaper<StateDto>>();
builder.Services.AddScoped<IDataShaper<CityDto>, DataShaper<CityDto>>();
builder.Services.AddScoped<IDataShaper<AddressDto>, DataShaper<AddressDto>>();
builder.Services.AddScoped<IDataShaper<TicketDto>, DataShaper<TicketDto>>();
builder.Services.AddScoped<IDataShaper<TicketGroupDto>, DataShaper<TicketGroupDto>>();
builder.Services.AddScoped<IDataShaper<TicketGroupWithTicketsDto>, DataShaper<TicketGroupWithTicketsDto>>();
builder.Services.AddScoped<IDataShaper<ReviewDto>, DataShaper<ReviewDto>>();
builder.Services.AddScoped<IDataShaper<CompanyDto>, DataShaper<CompanyDto>>();
builder.Services.AddScoped<IDataShaper<VehicleDto>, DataShaper<VehicleDto>>();
builder.Services.AddScoped<IDataShaper<VehicleEnrollmentDto>, DataShaper<VehicleEnrollmentDto>>();
builder.Services.AddScoped<IDataShaper<VehicleEnrollmentWithDetailsDto>, DataShaper<VehicleEnrollmentWithDetailsDto>>();
builder.Services.AddScoped<IDataShaper<RouteDto>, DataShaper<RouteDto>>();
builder.Services.AddScoped<IDataShaper<RouteWithAddressesDto>, DataShaper<RouteWithAddressesDto>>();
builder.Services.AddScoped<IDataShaper<RouteAddressDto>, DataShaper<RouteAddressDto>>();

builder.Services.AddScoped<IPager<ExpandoObject>, Pager<ExpandoObject>>();

builder.Services.AddScoped<AutomationService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IDataShaper<UserDto>, DataShaper<UserDto>>();
builder.Services.AddScoped<IDataShaper<ExpandoObject>, DataShaper<ExpandoObject>>();

// Adding DB Context with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Data seeding
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
await SeedData.Initialize(services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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