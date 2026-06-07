namespace JobApplicationManager.API.Features.Notifications.DTOs;

public class NotificationResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Read { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? DueAt { get; set; }
    public int? JobApplicationId { get; set; }
    public NotificationApplicationResponse? Application { get; set; }
}
