using System.ComponentModel.DataAnnotations;

namespace OffroadCamping.Appointments.Domain.Appointments
{
    public class Appointment : BaseEntity
    {
        public Appointment(
            Guid doctorId,
            Guid patientId,
            Guid appointmentId,
            DateTimeOffset start,
            string facilityName,
            string clientCalendarId)
        {
            DoctorId = doctorId;
            PatientId = patientId;
            AppointmentId = appointmentId;
            Start = start;
            FacilityName = facilityName;
            ClientCalendarId = clientCalendarId;
        }

        [Required]
        public Guid DoctorId { get; private init; }

        [Required]
        public Guid PatientId { get; private init; }

        [Required]
        public Guid AppointmentId { get; private init; }

        [Required]
        public DateTimeOffset Start { get; private init; }

        [Required]
        [MaxLength(100)]
        public string FacilityName { get; private init; }

        /// <summary>
        /// Currently this is a Google Calendar Event id.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ClientCalendarId { get; private set; }
    }
}
