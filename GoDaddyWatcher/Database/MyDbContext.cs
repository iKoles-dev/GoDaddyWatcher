using Microsoft.EntityFrameworkCore;

namespace GoDaddyWatcher.Database
{
    public class MyDbContext : DbContext
    {
        public DbSet<Site> Sites { get; set; }
        public MyDbContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Sites.db");
        }
    }
}