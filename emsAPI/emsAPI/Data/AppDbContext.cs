using Microsoft.EntityFrameworkCore;
using Models;

namespace emsAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceType>  DeviceTypes { get; set; }
    public DbSet<Employee>  Employees { get; set; }
    public DbSet<Loan>  Loans { get; set; }
    public DbSet<Producer>  Producers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}