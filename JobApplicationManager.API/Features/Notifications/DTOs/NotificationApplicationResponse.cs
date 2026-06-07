namespace JobApplicationManager.API.Features.Notifications.DTOs;

public class NotificationApplicationResponse
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
