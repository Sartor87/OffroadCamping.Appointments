namespace OffroadCamping.Appointments.Domain.User.Events
{
    public record UserCreatedEvent(
        Guid UserId,
        string Email,
        string FullName,
        string Role,
        DateTime CreatedAt
    );
}
