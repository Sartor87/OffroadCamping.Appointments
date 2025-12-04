using OffroadCamping.Appointments.Domain.User;

namespace OffroadCamping.Appointments.Application.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Get user by ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<User> GetUserByIdAsync(Guid userId);
    }
}
