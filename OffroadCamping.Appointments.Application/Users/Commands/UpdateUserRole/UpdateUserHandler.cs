using KurrentDB.Client;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Helpers;
using OffroadCamping.Appointments.Application.Redis;
using OffroadCamping.Appointments.Application.Services.Contracts;
using OffroadCamping.Appointments.Domain.User.Events;
using System.Text.Json;

namespace OffroadCamping.Appointments.Application.Users.Commands.UpdateUserRole
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserRoleCommand>
    {
        private readonly KurrentDBClient _eventStoreClient;
        private readonly IAuthService _authService;
        private readonly IDistributedCache _cache;

        public UpdateUserHandler(
            IAuthService authService,
            KurrentDBClient eventStoreClient,
            IDistributedCache cache
        )
        {
            _authService = authService;
            _eventStoreClient = eventStoreClient;
            _cache = cache;
        }

        public async Task Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceHelper.ActivitySource.StartActivity();

            var userUpdateRoleId = Guid.NewGuid();
            activity?.AddTag("UserUpdateRoleId", userUpdateRoleId);
            Domain.User.User? user = await GetCurrentUser(request, userUpdateRoleId, cancellationToken);
            await UpdateUserRole(request, userUpdateRoleId, user, cancellationToken);
        }

        /// <summary>
        /// Update user role
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userUpdateRoleId"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UpdateUserRole(UpdateUserRoleCommand request, Guid userUpdateRoleId, Domain.User.User? user, CancellationToken cancellationToken)
        {
            var newUserDto = new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                Role = request.NewRole
            };

            var updatedUser = await _authService.UpdateUserById(newUserDto, request.UserId);

            var userUpdatedEvent = new UserUpdatedEvent(userUpdateRoleId, updatedUser!.Id, updatedUser!.Username, updatedUser!.Email, updatedUser!.Role);

            var eventDataUpdated = new EventData(
                Uuid.NewUuid(),
                nameof(UserUpdatedEvent),
                JsonSerializer.SerializeToUtf8Bytes(userUpdatedEvent));

            await _eventStoreClient.AppendToStreamAsync(
                "userupdates_new",
                StreamState.Any,
                new[] { eventDataUpdated, },
                cancellationToken: cancellationToken
            );

            await _cache.RemoveAsync(CacheKeys.UserById(request.UserId));
        }

        private async Task<Domain.User.User?> GetCurrentUser(UpdateUserRoleCommand request, Guid userUpdateRoleId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserByIdAsync(request.UserId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var userGetEvent = new UserUpdatedEvent(userUpdateRoleId, user.Id, user.Username, user.Email, user.Role);

            var eventData = new EventData(
                Uuid.NewUuid(),
                nameof(UserUpdatedEvent),
                JsonSerializer.SerializeToUtf8Bytes(userGetEvent));

            await _eventStoreClient.AppendToStreamAsync(
                "userupdates_old",
                StreamState.Any,
                new[] { eventData, },
                cancellationToken: cancellationToken
            );
            return user;
        }
    }
}
