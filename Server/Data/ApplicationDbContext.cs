using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Route = Server.Models.Route;

namespace Server.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
    
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<VehicleEnrollment> VehicleEnrollments { get; set; } = null!;
    public DbSet<Route> Routes { get; set; } = null!;
    public DbSet<RouteAddress> RouteAddresses { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;

    public DbSet<RouteAddressDetails> RouteAddressDetails { get; set; } = null!;
    public DbSet<City> Cities { get; set; } = null!;
    public DbSet<State> States { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<TicketGroup> TicketGroups { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
}
