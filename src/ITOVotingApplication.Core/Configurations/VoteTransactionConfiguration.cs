using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class VoteTransactionConfiguration : IEntityTypeConfiguration<VoteTransaction>
	{
		public void Configure(EntityTypeBuilder<VoteTransaction> builder)
		{
			builder.ToTable("trVoteTransactions");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.Description)
				.HasMaxLength(500);

			builder.Property(e => e.CreatedDate)
				.HasDefaultValueSql("GETDATE()");

			// Relationships
			builder.HasOne(d => d.Company)
				.WithMany(p => p.VoteTransactions)
				.HasForeignKey(d => d.CompanyId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(d => d.Contact)
				.WithMany(p => p.VoteTransactions)
				.HasForeignKey(d => d.ContactId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(d => d.BallotBox)
				.WithMany(p => p.VoteTransactions)
				.HasForeignKey(d => d.BallotBoxId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(d => d.CreatedUser)
				.WithMany(p => p.CreatedVoteTransactions)
				.HasForeignKey(d => d.CreatedUserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Index - Bir kişi bir sandıkta sadece bir kez oy kullanabilir
			builder.HasIndex(e => new { e.ContactId, e.BallotBoxId }).IsUnique();
		}
	}
}