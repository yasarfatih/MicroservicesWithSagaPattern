using Microsoft.EntityFrameworkCore;

namespace Stock.API.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Stock> Stock{ get; set; }
    }
}
