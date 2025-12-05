using OffroadCamping.Appointments.SharedKernel.BusinessRulesEngine;
using System.Text.RegularExpressions;

namespace OffroadCamping.Appointments.Application.Appointments.Commands.CreateAppointment.BusinessRules
{
    internal class PatientValidEmailRule : IBusinessRule
    {
        private readonly string _email;

        private readonly Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

        internal PatientValidEmailRule(string email) => _email = email;

        public bool IsMet() => !string.IsNullOrWhiteSpace(_email) && regex.IsMatch(_email);

        public string Error => "Patient email is not valid.";
    }
}
