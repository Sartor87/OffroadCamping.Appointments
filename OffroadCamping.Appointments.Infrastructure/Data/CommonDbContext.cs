using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OffroadCamping.Appointments.Domain;

namespace OffroadCamping.Appointments.Infrastructure.Data
{
    public abstract class CommonDbContext : DbContext
    {

        public CommonDbContext(DbContextOptions options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            var time = DateTimeOffset.UtcNow;
            var createdEntities = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .OfType<BaseEntity>()
                .ToList();

            foreach (var entity in createdEntities)
            {
                entity.SetCreatedAt(time);
                entity.SetUpdatedAt(time);
            }

            var updatedEntities = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<BaseEntity>()
                .ToList();

            foreach (var entity in updatedEntities)
            {
                entity.SetUpdatedAt(time);
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
