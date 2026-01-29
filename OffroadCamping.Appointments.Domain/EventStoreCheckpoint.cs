using System.ComponentModel.DataAnnotations;

namespace OffroadCamping.Appointments.Domain
{
    public class EventStoreCheckpoint : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string StreamName { get; private init; } = "appointments";

        public long Position { get; private set; }

        public void UpdatePosition(long newPosition)
        {
            this.Position = newPosition;
        }
    }
}
