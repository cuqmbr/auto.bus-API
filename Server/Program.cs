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
using Route = Server.Models.Route;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options => {
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
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

var corsPolicyName = "defaultCorsPolicy";
builder.Services.AddCors(options => {
    options.AddPolicy(corsPolicyName,
        policy => policy.WithOrigins("http://localhost:4200").AllowCredentials()
            .AllowAnyHeader().AllowAnyMethod());
});

// Configuration from AppSettings
builder.Services.Configure<Jwt>(builder.Configuration.GetSection("Jwt"));
// Adding Authentication - JWT
builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        // options.RequireHttpsMetadata = false;
        // options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(typeof(MapperInitializer));

builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_.";
}).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddScoped<ICountryManagementService, CountryManagementService>();
builder.Services.AddScoped<IStateManagementService, StateManagementService>();
builder.Services.AddScoped<ICityManagementService, CityManagementService>();
builder.Services.AddScoped<IAddressManagementService, AddressManagementService>();
builder.Services.AddScoped<ITicketManagementService, TicketManagementService>();
builder.Services.AddScoped<IReviewManagementService, ReviewManagementService>();
builder.Services.AddScoped<ICompanyManagementService, CompanyManagementService>();
builder.Services.AddScoped<IVehicleManagementService, VehicleManagementService>();
builder.Services.AddScoped<IVehicleEnrollmentManagementService, VehicleEnrollmentManagementService>();
builder.Services.AddScoped<IRouteManagementService, RouteManagementService>();
builder.Services.AddScoped<IRouteAddressManagementService, RouteAddressManagementService>();

builder.Services.AddScoped<IStatisticsService, StatisticsService>();


builder.Services.AddScoped<ISortHelper<Country>, SortHelper<Country>>();
builder.Services.AddScoped<ISortHelper<State>, SortHelper<State>>();
builder.Services.AddScoped<ISortHelper<City>, SortHelper<City>>();
builder.Services.AddScoped<ISortHelper<Address>, SortHelper<Address>>();
builder.Services.AddScoped<ISortHelper<Ticket>, SortHelper<Ticket>>();
builder.Services.AddScoped<ISortHelper<Review>, SortHelper<Review>>();
builder.Services.AddScoped<ISortHelper<Company>, SortHelper<Company>>();
builder.Services.AddScoped<ISortHelper<Vehicle>, SortHelper<Vehicle>>();
builder.Services.AddScoped<ISortHelper<VehicleEnrollment>, SortHelper<VehicleEnrollment>>();
builder.Services.AddScoped<ISortHelper<Route>, SortHelper<Route>>();
builder.Services.AddScoped<ISortHelper<RouteAddress>, SortHelper<RouteAddress>>();

builder.Services.AddScoped<IDataShaper<User>, DataShaper<User>>();
builder.Services.AddScoped<IDataShaper<Country>, DataShaper<Country>>();
builder.Services.AddScoped<IDataShaper<State>, DataShaper<State>>();
builder.Services.AddScoped<IDataShaper<City>, DataShaper<City>>();
builder.Services.AddScoped<IDataShaper<Address>, DataShaper<Address>>();
builder.Services.AddScoped<IDataShaper<Ticket>, DataShaper<Ticket>>();
builder.Services.AddScoped<IDataShaper<Review>, DataShaper<Review>>();
builder.Services.AddScoped<IDataShaper<Company>, DataShaper<Company>>();
builder.Services.AddScoped<IDataShaper<Vehicle>, DataShaper<Vehicle>>();
builder.Services.AddScoped<IDataShaper<VehicleEnrollment>, DataShaper<VehicleEnrollment>>();
builder.Services.AddScoped<IDataShaper<Route>, DataShaper<Route>>();
builder.Services.AddScoped<IDataShaper<RouteAddress>, DataShaper<RouteAddress>>();

builder.Services.AddScoped<IDataShaper<UserDto>, DataShaper<UserDto>>();
builder.Services.AddScoped<IDataShaper<CompanyDto>, DataShaper<CompanyDto>>();

// Adding DB Context with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Data seeding
// using var scope = app.Services.CreateScope();
// var userManager = (UserManager<User>)scope.ServiceProvider.GetService(typeof(UserManager<User>))!;
// var roleManager = (RoleManager<IdentityRole>)scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>))!;
// await ApplicationDbContextSeed.SeedEssentialsAsync(userManager, roleManager);

app.MapControllers();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.Run();