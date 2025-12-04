using Microsoft.AspNetCore.Mvc;
using OffroadCamping.Appointments.Application.Dtos.Responses;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Services.Contracts;
using OffroadCamping.Appointments.Domain.User;
using System.Diagnostics;

namespace OffroadCamping.Appointments.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    ActivitySource source = new("OffroadCamping.Appointments");

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserDto request)
    {
        using var activity = source.StartActivity("User is registering with {Username}");
        activity?.SetTag("Username", request.Username);

        var user = await authService.RegisterAsync(request);

        if (user == null)
        {
            return BadRequest("User already exists.");
        }

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
    {

        if (request == null)
        {
            return BadRequest("No data passed.");
        }

        var response = await authService.LoginAsync(request);

        if (response == null)
        {
            return BadRequest("Invalid username or password.");
        }

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
    {
        var result = await authService.RefreshTokensAsync(request);

        if (result == null ||
            result.RefreshToken == null ||
            result.AccessToken == null)
        {
            return Unauthorized("Invalid refresh token");
        }

        return Ok(result);
    }
}
