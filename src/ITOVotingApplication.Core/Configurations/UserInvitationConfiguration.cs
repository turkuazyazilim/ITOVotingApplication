using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITOVotingApplication.Data.Configurations
{
    public class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
    {
        public void Configure(EntityTypeBuilder<UserInvitation> builder)
        {
            builder.ToTable("trUserInvitations");

            builder.HasKey(ui => ui.Id);

            builder.Property(ui => ui.Token)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ui => ui.Email)
                .HasMaxLength(100);

            builder.Property(ui => ui.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(ui => ui.CreatedDate)
                .IsRequired();

            builder.Property(ui => ui.ExpiryDate)
                .IsRequired();

            builder.Property(ui => ui.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            // Foreign key relationships
            builder.HasOne(ui => ui.CreatedByUser)
                .WithMany()
                .HasForeignKey(ui => ui.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ui => ui.UsedByUser)
                .WithMany()
                .HasForeignKey(ui => ui.UsedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(ui => ui.Token)
                .IsUnique();

            builder.HasIndex(ui => ui.CreatedDate);
            builder.HasIndex(ui => ui.ExpiryDate);
        }
    }
}