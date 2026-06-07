using JobApplicationManager.API.Features.Calendar.DTOs;

namespace JobApplicationManager.API.Features.Calendar.Interfaces;

public interface ICalendarEventService
{
    Task<List<CalendarEventResponse>> GetAllAsync(Guid userId);
    Task<List<CalendarEventResponse>> GetUpcomingAsync(Guid userId);
    Task<CalendarEventResponse?> GetByIdAsync(Guid userId, int id);
    Task<CalendarEventResponse> CreateAsync(Guid userId, CreateCalendarEventRequest request);
    Task<CalendarEventResponse?> UpdateAsync(Guid userId, int id, UpdateCalendarEventRequest request);
    Task<bool> DeleteAsync(Guid userId, int id);
}
