using JobApplicationManager.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManager.API.Data.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<TextBlock> TextBlocks => Set<TextBlock>();
    public DbSet<CoverLetterTemplate> CoverLetterTemplates => Set<CoverLetterTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}