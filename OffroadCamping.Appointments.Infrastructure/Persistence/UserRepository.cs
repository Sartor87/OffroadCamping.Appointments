using OffroadCamping.Appointments.Application.Repositories;
using OffroadCamping.Appointments.Domain.User;
using OffroadCamping.Appointments.Infrastructure.Data;

namespace OffroadCamping.Appointments.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(Guid userId) => await _context.Users.FindAsync(userId);


    }
}
