using OffroadCamping.Appointments.API.Endpoints.Auth;

namespace OffroadCamping.Appointments.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", RegisterEndpoint.Handle)
            .WithName("Register")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginEndpoint.Handle)
            .WithName("Login")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh-token", RefreshTokenEndpoint.Handle)
            .WithName("RefreshToken")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return routes;
    }
}