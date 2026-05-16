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
    public DbSet<CvDocument> CvDocuments => Set<CvDocument>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        
        modelBuilder.Entity<CvDocument>()
            .Property(x => x.Name)
            .HasMaxLength(200);

        modelBuilder.Entity<CvDocument>()
            .Property(x => x.OriginalFileName)
            .HasMaxLength(255);

        modelBuilder.Entity<CvDocument>()
            .Property(x => x.StoredFileName)
            .HasMaxLength(255);

        modelBuilder.Entity<CvDocument>()
            .Property(x => x.ContentType)
            .HasMaxLength(150);
    }
}