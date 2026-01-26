using OffroadCamping.Appointments.API.Endpoints.Appointments;

namespace OffroadCamping.Appointments.API.Endpoints;

public static class AppointmentsEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/appointments")
            .WithTags("Appointments")
            .RequireAuthorization("doctor");

        group.MapGet("/facilities/{facilityName}", GetAppointmentsEndpoint.Handle)
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(5)))
            .WithName("GetAppointments")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/", CreateAppointmentEndpoint.Handle)
            .WithName("CreateAppointment")
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return routes;
    }
}