using JobApplicationManager.API.Data.Context;
using JobApplicationManager.API.Data.Entities;
using JobApplicationManager.API.Features.Calendar.DTOs;
using JobApplicationManager.API.Features.Calendar.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManager.API.Features.Calendar.Services;

public class CalendarEventService : ICalendarEventService
{
    private readonly DataContext _context;

    public CalendarEventService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<CalendarEventResponse>> GetAllAsync(Guid userId)
    {
        var events = await _context.CalendarEvents
            .AsNoTracking()
            .Include(x => x.JobApplication)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync();

        return events
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<CalendarEventResponse>> GetUpcomingAsync(Guid userId)
    {
        var now = DateTime.UtcNow;

        var events = await _context.CalendarEvents
            .AsNoTracking()
            .Include(x => x.JobApplication)
            .Where(x => x.UserId == userId && x.StartDateTime >= now && !x.IsCompleted)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync();

        return events
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<CalendarEventResponse?> GetByIdAsync(Guid userId, int id)
    {
        var calendarEvent = await _context.CalendarEvents
            .AsNoTracking()
            .Include(x => x.JobApplication)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (calendarEvent is null)
        {
            return null;
        }

        return MapToResponse(calendarEvent);
    }

    public async Task<CalendarEventResponse> CreateAsync(Guid userId, CreateCalendarEventRequest request)
    {
        ValidateRequest(request.Title, request.StartDateTime, request.EndDateTime);

        var jobApplication = await GetJobApplicationOrThrowAsync(userId, request.JobApplicationId);

        var calendarEvent = new CalendarEvent
        {
            UserId = userId,
            JobApplicationId = request.JobApplicationId,
            JobApplication = jobApplication,
            Title = request.Title.Trim(),
            Description = request.Description,
            EventType = request.EventType,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            Location = request.Location,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync();

        return MapToResponse(calendarEvent);
    }

    public async Task<CalendarEventResponse?> UpdateAsync(Guid userId, int id, UpdateCalendarEventRequest request)
    {
        ValidateRequest(request.Title, request.StartDateTime, request.EndDateTime);

        var calendarEvent = await _context.CalendarEvents
            .Include(x => x.JobApplication)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (calendarEvent is null)
        {
            return null;
        }

        var jobApplication = await GetJobApplicationOrThrowAsync(userId, request.JobApplicationId);

        calendarEvent.UserId = userId;
        calendarEvent.JobApplicationId = request.JobApplicationId;
        calendarEvent.JobApplication = jobApplication;
        calendarEvent.Title = request.Title.Trim();
        calendarEvent.Description = request.Description;
        calendarEvent.EventType = request.EventType;
        calendarEvent.StartDateTime = request.StartDateTime;
        calendarEvent.EndDateTime = request.EndDateTime;
        calendarEvent.Location = request.Location;
        calendarEvent.IsCompleted = request.IsCompleted;
        calendarEvent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(calendarEvent);
    }

    public async Task<bool> DeleteAsync(Guid userId, int id)
    {
        var calendarEvent = await _context.CalendarEvents
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (calendarEvent is null)
        {
            return false;
        }

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<JobApplication?> GetJobApplicationOrThrowAsync(Guid userId, int? jobApplicationId)
    {
        if (!jobApplicationId.HasValue)
        {
            return null;
        }

        var jobApplication = await _context.JobApplications
            .FirstOrDefaultAsync(x => x.Id == jobApplicationId.Value && x.UserId == userId);

        if (jobApplication is null)
        {
            throw new ArgumentException("The linked job application does not exist.");
        }

        return jobApplication;
    }

    private static void ValidateRequest(string title, DateTime startDateTime, DateTime? endDateTime)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (endDateTime.HasValue && endDateTime.Value < startDateTime)
        {
            throw new ArgumentException("End date and time cannot be before start date and time.");
        }
    }

    private static CalendarEventResponse MapToResponse(CalendarEvent entity)
    {
        return new CalendarEventResponse
        {
            Id = entity.Id,
            JobApplicationId = entity.JobApplicationId,
            CompanyName = entity.JobApplication?.CompanyName,
            JobTitle = entity.JobApplication?.RoleTitle,
            Title = entity.Title,
            Description = entity.Description,
            EventType = entity.EventType,
            StartDateTime = entity.StartDateTime,
            EndDateTime = entity.EndDateTime,
            Location = entity.Location,
            IsCompleted = entity.IsCompleted,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
