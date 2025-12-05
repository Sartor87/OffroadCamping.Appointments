using OffroadCamping.Appointments.SharedKernel.BusinessRulesEngine;

namespace OffroadCamping.Appointments.Application.Appointments.Commands.CreateAppointment.BusinessRules
{
    internal class AppointmentStartDateRule : IBusinessRule
    {
        private readonly DateTime _startDate;
        public AppointmentStartDateRule(DateTime startDate)
        {
            _startDate = startDate;
        }
        public bool IsMet() => _startDate > DateTime.UtcNow;

        public string Error => "The appointment start date cannot be in the past.";
    }
}
