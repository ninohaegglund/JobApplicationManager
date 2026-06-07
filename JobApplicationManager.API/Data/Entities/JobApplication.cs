namespace JobApplicationManager.API.Data.Entities;

public class JobApplication
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public ICollection<ApplicationEmail> ApplicationEmails { get; set; } = new List<ApplicationEmail>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
