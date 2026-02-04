using InspectionService.Models;
using Microsoft.EntityFrameworkCore;

namespace InspectionService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TechnicalInspection> TechnicalInspections { get; set; }   
        public DbSet<LegalInspection> LegalInspections { get; set; }

    }
}
