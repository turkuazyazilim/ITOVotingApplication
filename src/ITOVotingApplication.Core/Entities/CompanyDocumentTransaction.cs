using ITOVotingApplication.Core.Enums;

namespace ITOVotingApplication.Core.Entities
{
    public class CompanyDocumentTransaction
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public CompanyDocumentType DocumentType { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public int UploadedByUserId { get; set; }

        // Delivery Status - only for Talep Dilekçesi
        public DocumentDeliveryStatus? DeliveryStatus { get; set; }
        public DateTime? DeliveryStatusDate { get; set; }

        // Rejection Info - only when DeliveryStatus = RedEdildi
        public DocumentRejectionReason? RejectionReason { get; set; }
        public string? RejectionNote { get; set; }

        // TC Company Election Participation
        public bool? WillParticipateInElection { get; set; }

        // Assigned Contact - only for Onaylanmış Yetki Belgesi
        public int? AssignedContactId { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; }
        public virtual User UploadedByUser { get; set; }
        public virtual Contact AssignedContact { get; set; }
    }
}
