using OffroadCamping.Appointments.Application.Dtos.Responses;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Services.Contracts;

namespace OffroadCamping.Appointments.API.Endpoints.Auth;

public static class LoginEndpoint
{
    public static async Task<IResult> Handle(
        UserDto request,
        IAuthService authService)
    {
        if (request == null)
            return Results.BadRequest("No data passed.");

        var response = await authService.LoginAsync(request);

        if (response == null)
            return Results.BadRequest("Invalid username or password.");

        return Results.Ok(response);
    }
}