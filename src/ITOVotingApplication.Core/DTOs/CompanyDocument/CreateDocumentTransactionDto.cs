using ITOVotingApplication.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Core.DTOs.CompanyDocument
{
    public class CreateDocumentTransactionDto
    {
        [Required]
        public int CompanyId { get; set; }

        [Required]
        public CompanyDocumentType DocumentType { get; set; }

        [Required]
        public string DocumentUrl { get; set; }

        // For TC companies - only when uploading request petition
        public bool? WillParticipateInElection { get; set; }

        // For Onaylanmış Yetki Belgesi - assign to a user
        public int? AssignedUserId { get; set; }
    }
}
