using DealService.Models;
using Microsoft.EntityFrameworkCore;


namespace DealService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Deal> Deals { get; set; }
    }
}
