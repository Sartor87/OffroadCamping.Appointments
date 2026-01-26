using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Services.Contracts;
using OffroadCamping.Appointments.Domain.User;
using System.Diagnostics;

namespace OffroadCamping.Appointments.API.Endpoints.Auth;

public static class RegisterEndpoint
{
    private static readonly ActivitySource Source = new("OffroadCamping.Appointments");

    public static async Task<IResult> Handle(
        UserDto request,
        IAuthService authService)
    {
        using var activity = Source.StartActivity("User is registering");
        activity?.SetTag("Username", request.Username);

        var user = await authService.RegisterAsync(request);

        if (user == null)
            return Results.BadRequest("User already exists.");

        return Results.Ok(user);
    }
}