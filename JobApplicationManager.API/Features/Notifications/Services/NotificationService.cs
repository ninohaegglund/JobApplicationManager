using System.Globalization;
using System.Text;
using JobApplicationManager.API.Data.Context;
using JobApplicationManager.API.Data.Entities;
using JobApplicationManager.API.Data.Enums;
using JobApplicationManager.API.Features.Notifications.DTOs;
using JobApplicationManager.API.Features.Notifications.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManager.API.Features.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly DataContext _context;

    public NotificationService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationResponse>> GetAllForUserAsync(Guid userId, bool unreadOnly = false)
    {
        await SyncGeneratedNotificationsAsync(userId);

        var query = _context.Notifications
            .AsNoTracking()
            .Include(x => x.JobApplication)
            .Where(x => x.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(x => !x.IsRead);
        }

        var notifications = await query
            .OrderBy(x => x.IsRead)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        return notifications
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        await SyncGeneratedNotificationsAsync(userId);

        return await _context.Notifications
            .CountAsync(x => x.UserId == userId && !x.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(Guid userId, int id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (notification is null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return 0;
        }

        var readAt = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = readAt;
        }

        await _context.SaveChangesAsync();

        return notifications.Count;
    }

    public async Task CreateApplicationUpdatedAsync(
        Guid userId,
        int jobApplicationId,
        string companyName,
        string roleTitle,
        string status)
    {
        var now = DateTime.UtcNow;
        var statusText = FormatStatus(status);

        var draftReminderSourceKey = GetDraftReminderSourceKey(jobApplicationId);
        var draftReminder = await _context.Notifications
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SourceKey == draftReminderSourceKey);

        if (draftReminder is not null && !draftReminder.IsRead)
        {
            draftReminder.IsRead = true;
            draftReminder.ReadAt = now;
        }

        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            JobApplicationId = jobApplicationId,
            Type = NotificationType.ApplicationUpdated,
            Title = "Application updated",
            Message = $"{companyName} moved your application to {statusText}.",
            CreatedAt = now
        });

        await _context.SaveChangesAsync();
    }

    private async Task SyncGeneratedNotificationsAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var candidates = new List<NotificationCandidate>();

        candidates.AddRange(await BuildCalendarReminderCandidatesAsync(userId, now));
        candidates.AddRange(await BuildDraftReminderCandidatesAsync(userId, now));

        await UpsertGeneratedNotificationsAsync(userId, candidates);
    }

    private async Task<List<NotificationCandidate>> BuildCalendarReminderCandidatesAsync(Guid userId, DateTime now)
    {
        var reminderStart = now.AddDays(-7);
        var reminderEnd = now.AddDays(2);

        var events = await _context.CalendarEvents
            .AsNoTracking()
            .Include(x => x.JobApplication)
            .Where(x =>
                x.UserId == userId &&
                !x.IsCompleted &&
                x.StartDateTime >= reminderStart &&
                x.StartDateTime <= reminderEnd)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync();

        return events
            .Select(x => BuildCalendarReminderCandidate(x, now))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    private async Task<List<NotificationCandidate>> BuildDraftReminderCandidatesAsync(Guid userId, DateTime now)
    {
        var cutoff = now.AddDays(-7);

        var applications = await _context.JobApplications
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.Status == "Draft" &&
                (x.UpdatedAt ?? x.CreatedAt) <= cutoff)
            .OrderBy(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync();

        return applications
            .Select(x =>
            {
                var lastActivityAt = x.UpdatedAt ?? x.CreatedAt;

                return new NotificationCandidate(
                    NotificationType.OldDraftReminder,
                    "Draft aging",
                    $"You have a draft application for {x.CompanyName} with no activity.",
                    x.Id,
                    lastActivityAt.AddDays(7),
                    GetDraftReminderSourceKey(x.Id),
                    now);
            })
            .ToList();
    }

    private async Task UpsertGeneratedNotificationsAsync(
        Guid userId,
        IReadOnlyCollection<NotificationCandidate> candidates,
        bool retryOnDuplicate = true)
    {
        if (candidates.Count == 0)
        {
            return;
        }

        var uniqueCandidates = candidates
            .GroupBy(x => x.SourceKey)
            .Select(x => x.OrderByDescending(candidate => candidate.CreatedAt).First())
            .ToList();

        var sourceKeys = candidates
            .Select(x => x.SourceKey)
            .Distinct()
            .ToList();

        var existingNotifications = await _context.Notifications
            .Where(x => x.UserId == userId && x.SourceKey != null && sourceKeys.Contains(x.SourceKey))
            .ToDictionaryAsync(x => x.SourceKey!);

        foreach (var candidate in uniqueCandidates)
        {
            if (existingNotifications.TryGetValue(candidate.SourceKey, out var existing))
            {
                existing.Type = candidate.Type;
                existing.Title = candidate.Title;
                existing.Message = candidate.Message;
                existing.JobApplicationId = candidate.JobApplicationId;
                existing.DueAt = candidate.DueAt;
                continue;
            }

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                JobApplicationId = candidate.JobApplicationId,
                Type = candidate.Type,
                Title = candidate.Title,
                Message = candidate.Message,
                DueAt = candidate.DueAt,
                SourceKey = candidate.SourceKey,
                CreatedAt = candidate.CreatedAt
            });
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (retryOnDuplicate && IsUniqueConstraintViolation(ex))
        {
            DetachAddedNotifications();
            await UpsertGeneratedNotificationsAsync(userId, uniqueCandidates, retryOnDuplicate: false);
        }
    }

    private void DetachAddedNotifications()
    {
        var addedNotifications = _context.ChangeTracker
            .Entries<Notification>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var notification in addedNotifications)
        {
            notification.State = EntityState.Detached;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
            sqlException.Errors
                .Cast<SqlError>()
                .Any(x => x.Number is 2601 or 2627);
    }

    private static NotificationCandidate? BuildCalendarReminderCandidate(CalendarEvent calendarEvent, DateTime now)
    {
        return calendarEvent.EventType switch
        {
            CalendarEventType.Interview when calendarEvent.StartDateTime >= now &&
                calendarEvent.StartDateTime <= now.AddDays(1) =>
                BuildInterviewReminderCandidate(calendarEvent, now),
            CalendarEventType.FollowUp when calendarEvent.StartDateTime <= now.AddDays(1) =>
                BuildFollowUpReminderCandidate(calendarEvent, now),
            CalendarEventType.Deadline when calendarEvent.StartDateTime <= now.AddDays(2) =>
                BuildDeadlineReminderCandidate(calendarEvent, now),
            CalendarEventType.Reminder when calendarEvent.StartDateTime <= now.AddDays(1) =>
                BuildReminderCandidate(calendarEvent, now),
            _ => null
        };
    }

    private static NotificationCandidate BuildInterviewReminderCandidate(CalendarEvent calendarEvent, DateTime now)
    {
        var relativeDate = GetRelativeDateText(calendarEvent.StartDateTime, now);
        var companyName = GetCompanyName(calendarEvent);

        return new NotificationCandidate(
            NotificationType.InterviewReminder,
            $"Interview {relativeDate}",
            $"Your {companyName} interview is scheduled for {relativeDate} at {calendarEvent.StartDateTime:HH:mm}.",
            calendarEvent.JobApplicationId,
            calendarEvent.StartDateTime,
            GetCalendarReminderSourceKey(calendarEvent.Id, NotificationType.InterviewReminder),
            now);
    }

    private static NotificationCandidate BuildFollowUpReminderCandidate(CalendarEvent calendarEvent, DateTime now)
    {
        var relativeDate = GetRelativeDateText(calendarEvent.StartDateTime, now);
        var companyName = GetCompanyName(calendarEvent);
        var title = calendarEvent.StartDateTime < now
            ? "Follow-up overdue"
            : $"Follow-up due {relativeDate}";

        return new NotificationCandidate(
            NotificationType.FollowUpReminder,
            title,
            $"Send a follow-up for {companyName}.",
            calendarEvent.JobApplicationId,
            calendarEvent.StartDateTime,
            GetCalendarReminderSourceKey(calendarEvent.Id, NotificationType.FollowUpReminder),
            now);
    }

    private static NotificationCandidate BuildDeadlineReminderCandidate(CalendarEvent calendarEvent, DateTime now)
    {
        var relativeDate = GetRelativeDateText(calendarEvent.StartDateTime, now);
        var companyName = GetCompanyName(calendarEvent);

        return new NotificationCandidate(
            NotificationType.DeadlineSoon,
            GetDeadlineTitle(calendarEvent.StartDateTime, now),
            $"The {companyName} application deadline is {relativeDate}.",
            calendarEvent.JobApplicationId,
            calendarEvent.StartDateTime,
            GetCalendarReminderSourceKey(calendarEvent.Id, NotificationType.DeadlineSoon),
            now);
    }

    private static NotificationCandidate BuildReminderCandidate(CalendarEvent calendarEvent, DateTime now)
    {
        var relativeDate = GetRelativeDateText(calendarEvent.StartDateTime, now);
        var title = calendarEvent.StartDateTime < now
            ? "Reminder overdue"
            : $"Reminder {relativeDate}";

        return new NotificationCandidate(
            NotificationType.Reminder,
            title,
            GetReminderMessage(calendarEvent),
            calendarEvent.JobApplicationId,
            calendarEvent.StartDateTime,
            GetCalendarReminderSourceKey(calendarEvent.Id, NotificationType.Reminder),
            now);
    }

    private static string GetDeadlineTitle(DateTime deadline, DateTime now)
    {
        if (deadline < now)
        {
            return "Deadline passed";
        }

        if (deadline.Date == now.Date)
        {
            return "Deadline today";
        }

        if (deadline <= now.AddDays(2))
        {
            return "Deadline in 48 hours";
        }

        return "Deadline soon";
    }

    private static string GetRelativeDateText(DateTime dateTime, DateTime now)
    {
        if (dateTime < now)
        {
            return "overdue";
        }

        if (dateTime.Date == now.Date)
        {
            return "today";
        }

        if (dateTime.Date == now.Date.AddDays(1))
        {
            return "tomorrow";
        }

        return $"on {dateTime.ToString("MMM d", CultureInfo.InvariantCulture)}";
    }

    private static string GetCompanyName(CalendarEvent calendarEvent)
    {
        return calendarEvent.JobApplication?.CompanyName ?? "this application";
    }

    private static string GetReminderMessage(CalendarEvent calendarEvent)
    {
        if (!string.IsNullOrWhiteSpace(calendarEvent.Description))
        {
            return calendarEvent.Description.Trim();
        }

        if (calendarEvent.JobApplication is not null)
        {
            return $"Reminder for {calendarEvent.JobApplication.CompanyName}: {calendarEvent.Title}.";
        }

        return $"Reminder: {calendarEvent.Title}.";
    }

    private static string FormatStatus(string status)
    {
        var trimmedStatus = status.Trim();
        if (string.IsNullOrWhiteSpace(trimmedStatus))
        {
            return "the next stage";
        }

        var builder = new StringBuilder(trimmedStatus.Length + 4);
        for (var index = 0; index < trimmedStatus.Length; index++)
        {
            var current = trimmedStatus[index];
            var previous = index > 0 ? trimmedStatus[index - 1] : '\0';

            if (index > 0 && char.IsUpper(current) && char.IsLower(previous))
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }

    private static string GetCalendarReminderSourceKey(int calendarEventId, NotificationType notificationType)
    {
        return $"calendar-event:{calendarEventId}:{notificationType}";
    }

    private static string GetDraftReminderSourceKey(int jobApplicationId)
    {
        return $"draft-aging:{jobApplicationId}";
    }

    private static string GetNotificationTypeLabel(NotificationType type)
    {
        return type switch
        {
            NotificationType.InterviewReminder => "Interview reminder",
            NotificationType.FollowUpReminder => "Follow-up reminder",
            NotificationType.DeadlineSoon => "Deadline soon",
            NotificationType.ApplicationUpdated => "Application updated",
            NotificationType.OldDraftReminder => "Old draft reminder",
            NotificationType.Reminder => "Reminder",
            _ => "General"
        };
    }

    private static NotificationResponse MapToResponse(Notification entity)
    {
        return new NotificationResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            Type = GetNotificationTypeLabel(entity.Type),
            Read = entity.IsRead,
            CreatedAt = entity.CreatedAt,
            ReadAt = entity.ReadAt,
            DueAt = entity.DueAt,
            JobApplicationId = entity.JobApplicationId,
            Application = entity.JobApplication is null
                ? null
                : new NotificationApplicationResponse
                {
                    Id = entity.JobApplication.Id,
                    Company = entity.JobApplication.CompanyName,
                    Role = entity.JobApplication.RoleTitle
                }
        };
    }

    private sealed record NotificationCandidate(
        NotificationType Type,
        string Title,
        string Message,
        int? JobApplicationId,
        DateTime? DueAt,
        string SourceKey,
        DateTime CreatedAt);
}
