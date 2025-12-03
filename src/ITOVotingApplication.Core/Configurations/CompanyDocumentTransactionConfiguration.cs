using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITOVotingApplication.Core.Entities;

namespace ITOVotingApplication.Data.Configurations
{
	public class CompanyDocumentTransactionConfiguration : IEntityTypeConfiguration<CompanyDocumentTransaction>
	{
		public void Configure(EntityTypeBuilder<CompanyDocumentTransaction> builder)
		{
			builder.ToTable("trCompanyDocumentTransactions");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.CompanyId)
				.IsRequired();

			builder.Property(e => e.DocumentType)
				.IsRequired();

			builder.Property(e => e.DocumentUrl)
				.HasMaxLength(500)
				.IsRequired();

			builder.Property(e => e.UploadDate)
				.IsRequired();

			builder.Property(e => e.UploadedByUserId)
				.IsRequired();

			builder.Property(e => e.DeliveryStatus);

			builder.Property(e => e.DeliveryStatusDate);

			builder.Property(e => e.RejectionReason);

			builder.Property(e => e.RejectionNote)
				.HasMaxLength(500);

			builder.Property(e => e.WillParticipateInElection);

			builder.Property(e => e.AssignedUserId);

			builder.Property(e => e.CreatedDate)
				.IsRequired();

			builder.Property(e => e.UpdatedDate);

			// Relationships
			builder.HasOne(d => d.Company)
				.WithMany()
				.HasForeignKey(d => d.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(d => d.UploadedByUser)
				.WithMany()
				.HasForeignKey(d => d.UploadedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(d => d.AssignedUser)
				.WithMany()
				.HasForeignKey(d => d.AssignedUserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Indexes
			builder.HasIndex(e => e.CompanyId);
			builder.HasIndex(e => e.UploadedByUserId);
			builder.HasIndex(e => e.AssignedUserId);
			builder.HasIndex(e => new { e.CompanyId, e.DocumentType });
		}
	}
}
