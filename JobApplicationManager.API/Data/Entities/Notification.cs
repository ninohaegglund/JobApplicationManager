using JobApplicationManager.API.Data.Enums;

namespace JobApplicationManager.API.Data.Entities;

public class Notification
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int? JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public NotificationType Type { get; set; } = NotificationType.General;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public DateTime? DueAt { get; set; }
    public string? SourceKey { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
