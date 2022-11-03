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
    
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<VehicleEnrollment> VehicleEnrollments { get; set; } = null!;
    public DbSet<Route> Routes { get; set; } = null!;
    public DbSet<RouteAddress> RouteAddresses { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<City> Cities { get; set; } = null!;
    public DbSet<State> States { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<RouteAddress>()
            .HasKey(ra => new {ra.RouteId, ra.AddressId});

        modelBuilder.Entity<Ticket>()
            .HasKey(t => new {t.UserId, t.VehicleEnrollmentId});
        
        modelBuilder.Entity<Review>()
            .HasKey(t => new {t.UserId, t.VehicleEnrollmentId});
    }
}