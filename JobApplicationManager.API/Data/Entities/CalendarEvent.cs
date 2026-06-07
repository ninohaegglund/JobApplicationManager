using JobApplicationManager.API.Data.Enums;

namespace JobApplicationManager.API.Data.Entities;

public class CalendarEvent
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int? JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public CalendarEventType EventType { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public string? Location { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


