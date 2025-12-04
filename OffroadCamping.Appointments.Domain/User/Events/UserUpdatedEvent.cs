namespace OffroadCamping.Appointments.Domain.User.Events
{
    public record UserUpdatedEvent(Guid Id, Guid UserId, string Username, string Email, string Role);
}
