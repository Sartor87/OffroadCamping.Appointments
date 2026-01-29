using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OffroadCamping.Appointments.Domain
{
    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private init; }

        [Required]
        public DateTimeOffset CreatedAt { get; private set; }

        [Required]
        public DateTimeOffset UpdatedAt { get; private set; }

        public void SetCreatedAt(DateTimeOffset createdAt)
        {
            this.CreatedAt = createdAt;
        }

        public void SetUpdatedAt(DateTimeOffset updatedAt)
        {
            this.UpdatedAt = updatedAt;
        }
    }
}
