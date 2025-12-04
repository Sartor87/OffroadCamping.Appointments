using MediatR;

namespace OffroadCamping.Appointments.Application.Users.Commands.UpdateUserRole
{
    public record UpdateUserRoleCommand (
        Guid UserId,
        string NewRole
    ) : IRequest;
}