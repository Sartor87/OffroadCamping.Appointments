using OffroadCamping.Appointments.Application.Dtos.Responses;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Domain.User;

namespace OffroadCamping.Appointments.Application.Services.Contracts
{
    public interface IAuthService
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> UpdateUserById(UserDto request, Guid userId);
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}
