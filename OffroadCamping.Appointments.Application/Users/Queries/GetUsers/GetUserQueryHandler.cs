using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Helpers;
using OffroadCamping.Appointments.Application.Redis;
using OffroadCamping.Appointments.Application.Repositories;
using System.Text.Json;

namespace OffroadCamping.Appointments.Application.Users.Queries.GetUsers
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDistributedCache _cache;

        public GetUserQueryHandler(IUserRepository userRepository, IDistributedCache cache)
        {
            _userRepository = userRepository;
            _cache = cache;
        }

        public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceHelper.ActivitySource.StartActivity();
            var cacheKey = CacheKeys.UserById(request.UserId);
            var cachedUser = await _cache.GetStringAsync(cacheKey);

            if (cachedUser != null)
            {
                return JsonSerializer.Deserialize<UserDto>(cachedUser)!;
            }

            var user = await GetUserFromDb(request);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            return user;
        }

        private async Task<UserDto> GetUserFromDb(GetUserQuery request)
        {
            using var activity = ActivitySourceHelper.ActivitySource.StartActivity();
            var user = await _userRepository.GetUserByIdAsync(request.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
            }

            return new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
