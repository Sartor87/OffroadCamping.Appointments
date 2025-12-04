using Microsoft.EntityFrameworkCore;
using OffroadCamping.Appointments.Domain;
using OffroadCamping.Appointments.Domain.Appointments;

namespace OffroadCamping.Appointments.Infrastructure.Data
{
    public class AppointmentsDbContext : CommonDbContext
    {
        private const string SchemaName = "Appointments";

        public AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options)
        : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<EventStoreCheckpoint> EventStoreCheckpoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.Entity<Appointment>().HasKey(x => x.Id);
        }
    }
}
