using System.ComponentModel.DataAnnotations;

namespace OffroadCamping.Appointments.Domain
{
    public class EventStoreCheckpoint : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string StreamName { get; init; } = "appointments";

        public long Position { get; set; }
    }
}
