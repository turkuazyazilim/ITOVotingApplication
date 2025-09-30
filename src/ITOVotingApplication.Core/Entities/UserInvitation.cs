using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Core.Entities
{
    public class UserInvitation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Token { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }

        public DateTime? UsedDate { get; set; }

        public int? UsedByUserId { get; set; }

        public int CreatedByUserId { get; set; }

        // Navigation properties
        public virtual User? UsedByUser { get; set; }
        public virtual User CreatedByUser { get; set; }
    }
}