using Microsoft.EntityFrameworkCore;

public class DeviceDbContext : DbContext
{
    public DbSet<Device> Device { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=./Database/DeviceDatabase.db");
}