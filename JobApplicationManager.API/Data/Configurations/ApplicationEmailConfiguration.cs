using JobApplicationManager.API.Data.Entities;
using JobApplicationManager.API.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManager.API.Data.Configurations;

public class ApplicationEmailConfiguration : IEntityTypeConfiguration<ApplicationEmail>
{
    public void Configure(EntityTypeBuilder<ApplicationEmail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Sender)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Body)
            .IsRequired();

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasDefaultValue(EmailType.General);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.ApplicationEmails)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.JobApplicationId);
    }
}
