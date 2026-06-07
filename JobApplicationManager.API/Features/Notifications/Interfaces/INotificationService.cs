using JobApplicationManager.API.Features.Notifications.DTOs;

namespace JobApplicationManager.API.Features.Notifications.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetAllForUserAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid userId, int id);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task CreateApplicationUpdatedAsync(
        Guid userId,
        int jobApplicationId,
        string companyName,
        string roleTitle,
        string status);
}
