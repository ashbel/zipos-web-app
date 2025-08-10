using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POS.Infrastructure.Data;

public class ControlPlaneDbContextFactory : IDesignTimeDbContextFactory<ControlPlaneDbContext>
{
    public ControlPlaneDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ControlPlaneDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("CONTROLPLANE_CONNECTION")
                              ?? "Host=localhost;Database=modernpos_control;Username=postgres;Password=mysecretpassword";
        optionsBuilder.UseNpgsql(connectionString);
        return new ControlPlaneDbContext(optionsBuilder.Options);
    }
}


