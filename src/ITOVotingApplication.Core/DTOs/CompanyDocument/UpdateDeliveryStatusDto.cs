using ITOVotingApplication.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Core.DTOs.CompanyDocument
{
    public class UpdateDeliveryStatusDto
    {
        [Required]
        public int TransactionId { get; set; }

        [Required]
        public DocumentDeliveryStatus DeliveryStatus { get; set; }

        // Required only when DeliveryStatus = RedEdildi
        public DocumentRejectionReason? RejectionReason { get; set; }

        // Optional note for rejection
        public string? RejectionNote { get; set; }
    }
}
