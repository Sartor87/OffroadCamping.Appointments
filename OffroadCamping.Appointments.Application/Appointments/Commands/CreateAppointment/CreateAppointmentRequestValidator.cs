using FluentValidation;

namespace OffroadCamping.Appointments.Application.Appointments.Commands.CreateAppointment
{
    internal class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentCommand>
    {
        public CreateAppointmentRequestValidator()
        {
            RuleFor(x => x.FacilityName)
                .NotEmpty().WithMessage("Facility name is required.")
                .MaximumLength(100).WithMessage("Facility name must not exceed 100 characters.");
            RuleFor(x => x.DoctorName)
                .NotEmpty().WithMessage("Doctor name is required.")
                .MaximumLength(100).WithMessage("Doctor name must not exceed 100 characters.");
            RuleFor(x => x.PatientEmail)
                .NotEmpty().WithMessage("Patient email is required.")
                .EmailAddress().WithMessage("Patient email must be a valid email address.")
                .MaximumLength(100).WithMessage("Patient email must not exceed 100 characters.");
            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage("Doctor ID is required.");
            RuleFor(x => x.PatientId)
                .NotEmpty().WithMessage("Patient ID is required.");
            RuleFor(x => x.Start)
                .GreaterThan(DateTime.Now).WithMessage("Appointment start time must be in the future.");
        }
    }
}
