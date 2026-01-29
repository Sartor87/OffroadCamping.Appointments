using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OffroadCamping.Appointments.Application.Dtos.Responses;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Services.Contracts;
using OffroadCamping.Appointments.Domain.User;
using OffroadCamping.Appointments.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OffroadCamping.Appointments.Infrastructure.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await context.Users.FindAsync(userId);
        }

        public async Task<User?> UpdateUserById(UserDto request, Guid userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            user.UpdateUser(request.Email, request.Username, request.Role);
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
            {
                return null;
            }

            var userPasswordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (userPasswordVerificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return null;
            }

            var user = new User();

            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            user.UpdateUser(user.Email, request.Username, request.Role, hashedPassword);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }
        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);

            if (user == null)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        #region Private methods

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow) { return null; }

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshUserToken(refreshToken, DateTime.UtcNow.AddMinutes(10));
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("JNumber", user.InternalSystemUserId!)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        private async Task<TokenResponseDto?> CreateTokenResponse(User user)
        {
            string token = CreateToken(user);

            return new TokenResponseDto
            {
                AccessToken = token,
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }
        #endregion
    }
}
