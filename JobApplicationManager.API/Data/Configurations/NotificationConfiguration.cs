using JobApplicationManager.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManager.API.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.SourceKey)
            .HasMaxLength(250);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
        builder.HasIndex(x => x.JobApplicationId);
        builder.HasIndex(x => new { x.UserId, x.SourceKey })
            .IsUnique()
            .HasFilter("[SourceKey] IS NOT NULL");
    }
}
