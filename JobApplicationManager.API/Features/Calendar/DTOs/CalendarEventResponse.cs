using JobApplicationManager.API.Data.Enums;

namespace JobApplicationManager.API.Features.Calendar.DTOs;

public class CalendarEventResponse
{
    public int Id { get; set; }

    public int? JobApplicationId { get; set; }
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public CalendarEventType EventType { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public string? Location { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
