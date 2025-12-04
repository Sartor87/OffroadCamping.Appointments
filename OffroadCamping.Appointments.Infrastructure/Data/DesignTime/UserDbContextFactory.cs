using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OffroadCamping.Appointments.Infrastructure.Data.DesignTime
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=IdentityDb;User Id=sa;Password=YourLocalPassword;TrustServerCertificate=True;");

            return new UserDbContext(optionsBuilder.Options);
        }
    }
}
