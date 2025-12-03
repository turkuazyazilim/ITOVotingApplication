using ITOVotingApplication.Core.Enums;

namespace ITOVotingApplication.Core.DTOs.CompanyDocument
{
    public class CompanyDocumentTransactionDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public CompanyDocumentType DocumentType { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public int UploadedByUserId { get; set; }
        public string UploadedByUserName { get; set; }

        // Delivery Status
        public DocumentDeliveryStatus? DeliveryStatus { get; set; }
        public string? DeliveryStatusName { get; set; }
        public DateTime? DeliveryStatusDate { get; set; }

        // Rejection Info
        public DocumentRejectionReason? RejectionReason { get; set; }
        public string? RejectionReasonName { get; set; }
        public string? RejectionNote { get; set; }

        // TC Company Election Participation
        public bool? WillParticipateInElection { get; set; }

        // Assigned User
        public int? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
