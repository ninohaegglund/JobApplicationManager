using System.Security.Claims;
using JobApplicationManager.API.Features.Notifications.DTOs;
using JobApplicationManager.API.Features.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManager.API.Features.Notifications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> GetAll([FromQuery] bool unreadOnly = false)
    {
        var userId = GetUserId();

        var notifications = await _notificationService.GetAllForUserAsync(userId, unreadOnly);

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<NotificationUnreadCountResponse>> GetUnreadCount()
    {
        var userId = GetUserId();
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        return Ok(new NotificationUnreadCountResponse
        {
            UnreadCount = unreadCount
        });
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        var updated = await _notificationService.MarkAsReadAsync(userId, id);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<NotificationUnreadCountResponse>> MarkAllAsRead()
    {
        var userId = GetUserId();
        var updatedCount = await _notificationService.MarkAllAsReadAsync(userId);

        return Ok(new NotificationUnreadCountResponse
        {
            UnreadCount = updatedCount
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user ID claim.");
        }

        return userId;
    }
}
