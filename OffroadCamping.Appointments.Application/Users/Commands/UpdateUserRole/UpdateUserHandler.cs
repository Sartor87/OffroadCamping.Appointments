using KurrentDB.Client;
using MediatR;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Helpers;
using OffroadCamping.Appointments.Application.Services.Contracts;
using OffroadCamping.Appointments.Domain.User.Events;
using System.Text.Json;

namespace OffroadCamping.Appointments.Application.Users.Commands.UpdateUserRole
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserRoleCommand>
    {
        private readonly KurrentDBClient _eventStoreClient;
        private readonly IAuthService _authService;

        public UpdateUserHandler(
            IAuthService authService,
            KurrentDBClient eventStoreClient
        )
        {
            _authService = authService;
            _eventStoreClient = eventStoreClient;
        }

        public async Task Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceHelper.ActivitySource.StartActivity();

            var userUpdateRoleId = Guid.NewGuid();
            activity?.AddTag("UserUpdateRoleId", userUpdateRoleId);

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
        }

    }
}
