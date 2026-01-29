namespace OffroadCamping.Appointments.Domain.User
{
    public class User : BaseEntity
    {
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;

        public string Role { get; private set; } = string.Empty;

        public string? InternalSystemUserId { get; private set; }

        public string? RefreshToken { get; private set; }

        public DateTime? RefreshTokenExpiryTime { get; private set; }

        public void UpdateUser(string newEmail, string newUserName, string newRole, string newHashedPassword = "")
        {

            this.Username = newUserName;
            this.Email = newEmail;
            this.PasswordHash = String.IsNullOrWhiteSpace(newHashedPassword) ? this.PasswordHash : newHashedPassword;
            this.Role = newRole;
        }

        public void RefreshUserToken(string refreshToken, DateTime tokenExpiryTime)
        {

            this.RefreshToken = refreshToken;
            this.RefreshTokenExpiryTime = tokenExpiryTime;
            
        }
    }
}
