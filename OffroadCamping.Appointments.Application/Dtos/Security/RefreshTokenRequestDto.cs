namespace OffroadCamping.Appointments.Application.Dtos.Security
{
    public class RefreshTokenRequestDto
    {
        public Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}
