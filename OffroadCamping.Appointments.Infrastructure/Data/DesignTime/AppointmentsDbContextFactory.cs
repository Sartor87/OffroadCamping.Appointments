using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OffroadCamping.Appointments.Infrastructure.Data;

public class AppointmentsDbContextFactory : IDesignTimeDbContextFactory<AppointmentsDbContext>
{
    public AppointmentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppointmentsDbContext>();

        // Use a LOCAL connection string for migrations
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=AppointmentsDb;User Id=sa;Password=YourLocalPassword;TrustServerCertificate=True;");

        return new AppointmentsDbContext(optionsBuilder.Options);
    }
}