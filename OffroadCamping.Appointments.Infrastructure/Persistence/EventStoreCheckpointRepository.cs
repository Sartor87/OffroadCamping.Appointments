using Microsoft.EntityFrameworkCore;
using OffroadCamping.Appointments.Application.Repositories;
using OffroadCamping.Appointments.Infrastructure.Data;

namespace OffroadCamping.Appointments.Infrastructure.Persistence
{
    public class EventStoreCheckpointRepository : IEventStoreCheckpointRepository
    {
        private readonly AppointmentsDbContext _context;

        public EventStoreCheckpointRepository(AppointmentsDbContext context)
        {
            _context = context;
        }

        public async Task<long> GetCheckpoint(string streamName)
        {
            var checkpoint = await _context.EventStoreCheckpoints.FirstOrDefaultAsync(x => x.StreamName == streamName);
            return checkpoint?.Position ?? 0;
        }

        public async Task IncrementCheckpoint(string streamName)
        {
            var checkpoint = await _context.EventStoreCheckpoints.FirstAsync(x => x.StreamName == streamName);
            checkpoint.UpdatePosition(checkpoint.Position + 1);
            await _context.SaveChangesAsync();
        }
    }
}
