using OffroadCamping.Appointments.Application.Dtos.Responses;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Services.Contracts;

namespace OffroadCamping.Appointments.API.Endpoints.Auth;

public static class RefreshTokenEndpoint
{
    public static async Task<IResult> Handle(
        RefreshTokenRequestDto request,
        IAuthService authService)
    {
        var result = await authService.RefreshTokensAsync(request);

        if (result == null ||
            result.RefreshToken == null ||
            result.AccessToken == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result);
    }
}   