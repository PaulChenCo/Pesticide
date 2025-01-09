using Microsoft.EntityFrameworkCore;

namespace PesticideContext;

public class DatabaseContext : DbContext
{
 
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { 
        
    }
    public DbSet<Pesticide> Pesticide { get; set; }
    public DbSet<Register> Register { get; set; }
    public DbSet<Product> Product { get; set; }

    public DbSet<ThirdParty> ThirdParty { get; set; }   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Pesticide>().HasNoKey();
        modelBuilder.Entity<ThirdParty>().HasNoKey();
    }
}