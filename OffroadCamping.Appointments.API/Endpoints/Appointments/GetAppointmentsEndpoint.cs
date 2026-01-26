using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using OffroadCamping.Appointments.Application.Appointments.Queries.GetAppointments;
using OffroadCamping.Appointments.Application.Helpers;
using System.Security.Claims;

namespace OffroadCamping.Appointments.API.Endpoints.Appointments;

public static class GetAppointmentsEndpoint
{
    public static async Task<IResult> Handle(
        string facilityName,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        using var activity = ActivitySourceHelper.ActivitySource.StartActivity();

        var doctorIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(doctorIdClaim, out var doctorId))
            return Results.Problem("Doctor ID not found in token.");

        var facilitiesDoctorBelongsTo = user.Claims.Where(c => c.Type == "medical_facilities");
        if (!facilitiesDoctorBelongsTo.Any(x => x.Value == facilityName))
            return Results.Unauthorized();

        var appointments = await mediator.Send(new GetAppointmentsQuery(facilityName, doctorId));
        return Results.Ok(appointments);
    }
}