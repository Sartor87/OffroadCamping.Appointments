using MediatR;
using OffroadCamping.Appointments.Domain.Appointments.Events;

namespace OffroadCamping.Appointments.Application.Appointments.Commands.CreateAppointment;

/// <summary>
/// Create an appointment command.
/// </summary>
/// <param name="Start"></param>
/// <param name="PatientEmail"></param>
/// <param name="Summary"></param>
/// <param name="Description"></param>
/// <param name="FacilityName">To come in v2.</param>
/// <param name="DoctorName"></param>
public record CreateAppointmentCommand(
    DateTime Start,
    string PatientEmail,
    string Summary,
    string Description,
    string FacilityName,
    string DoctorName,
    Guid PatientId,
    Guid DoctorId) : IRequest;