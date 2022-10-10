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
    }
    
    public DbSet<Company> Companies { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleEnrollment> VehicleEnrollments { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<RouteAddress> RouteAddresses { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<State> States { get; set; }
    public DbSet<Country> Countries { get; set; }
    
    public DbSet<Ticket> Tickets { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<RouteAddress>()
            .HasKey(ra => new {ra.RouteId, ra.AddressId});

        modelBuilder.Entity<Ticket>()
            .HasKey(t => new {t.UserId, t.VehicleEnrollmentId});
    }
}