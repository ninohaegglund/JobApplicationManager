using JobApplicationManager.API.Features.Calendar.DTOs;

namespace JobApplicationManager.API.Features.Calendar.Interfaces;

public interface ICalendarEventService
{
    Task<List<CalendarEventResponse>> GetAllAsync();
    Task<List<CalendarEventResponse>> GetUpcomingAsync();
    Task<CalendarEventResponse?> GetByIdAsync(int id);
    Task<CalendarEventResponse> CreateAsync(CreateCalendarEventRequest request);
    Task<CalendarEventResponse?> UpdateAsync(int id, UpdateCalendarEventRequest request);
    Task<bool> DeleteAsync(int id);
}
