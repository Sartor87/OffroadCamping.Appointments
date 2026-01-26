using MediatR;
using Microsoft.AspNetCore.Authorization;
using OffroadCamping.Appointments.Application.Appointments.Commands.CreateAppointment;
using OffroadCamping.Appointments.Application.Dtos.Security;
using OffroadCamping.Appointments.Application.Helpers;
using OffroadCamping.Appointments.Application.Services.Contracts;
using OffroadCamping.Appointments.Application.Users.Queries.GetUsers;
using OffroadCamping.Appointments.Dtos.Requests;
using System.Security.Claims;

namespace OffroadCamping.Appointments.API.Endpoints.Appointments;

public static class CreateAppointmentEndpoint
{
    public static async Task<IResult> Handle(
        CreateAppointmentRequestDto request,
        ClaimsPrincipal user,
        IMediator mediator,
        ICalendarService calendarService)
    {
        using var activity = ActivitySourceHelper.ActivitySource.StartActivity();

        var doctorName = user.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
        var doctorEmail = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var doctorIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (doctorName == null || doctorEmail == null || !Guid.TryParse(doctorIdClaim, out var doctorId))
            return Results.Problem("Doctor name, email or ID missing from token.");

        var facilitiesDoctorBelongsTo = user.Claims.Where(c => c.Type == "medical_facilities");
        if (!facilitiesDoctorBelongsTo.Any(x => x.Value == request.FacilityName))
            return Results.Unauthorized();

        var isSlotAvailable = await calendarService.IsAppointmentSlotAvailable(request.Start);
        if (!isSlotAvailable)
            return Results.BadRequest("Appointment slot is not available. Ensure 30 minutes between appointments.");

        var patient = await mediator.Send(new GetUserQuery(request.PatientId, "", ""));
        if (patient is not UserDto userDto)
            return Results.NotFound("Patient not found.");

        var command = new CreateAppointmentCommand(
            request.Start,
            userDto.Email,
            request.Summary,
            request.Description,
            request.FacilityName,
            doctorName,
            request.PatientId,
            doctorId);

        await mediator.Send(command);

        return Results.Accepted();
    }
}