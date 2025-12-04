using Microsoft.EntityFrameworkCore;
using OffroadCamping.Appointments.Domain.User;

namespace OffroadCamping.Appointments.Infrastructure.Data
{
    public class UserDbContext : CommonDbContext
    {
        private const string SchemaName = "Users";
        public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.Entity<User>().HasKey(x => x.Id);
        }
    }
}
