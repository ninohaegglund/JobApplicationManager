using JobApplicationManager.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationManager.API.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(x => x.AddressLine)
            .HasMaxLength(200);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        builder.Property(x => x.LegalGender)
            .HasMaxLength(50);

        builder.Property(x => x.LinkedInUrl)
            .HasMaxLength(300);

        builder.Property(x => x.GitHubUrl)
            .HasMaxLength(300);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}