using Microsoft.EntityFrameworkCore;
using VehicleSearchService.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace VehicleSearchService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { } 

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleSelection> VehicleSelections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleSelection>()
                .HasMany(vs => vs.Vehicles)
                .WithMany();
        }
    }
}
