using System.Security.Claims;
using JobApplicationManager.API.Features.Calendar.DTOs;
using JobApplicationManager.API.Features.Calendar.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManager.API.Features.Calendar;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarEventsController : ControllerBase
{
    private readonly ICalendarEventService _calendarEventService;

    public CalendarEventsController(ICalendarEventService calendarEventService)
    {
        _calendarEventService = calendarEventService;
    }

    [HttpPost]
    public async Task<ActionResult<CalendarEventResponse>> Create([FromBody] CreateCalendarEventRequest request)
    {
        var userId = GetUserId();

        try
        {
            var createdEvent = await _calendarEventService.CreateAsync(userId, request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdEvent.Id },
                createdEvent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<CalendarEventResponse>>> GetAll()
    {
        var userId = GetUserId();
        var events = await _calendarEventService.GetAllAsync(userId);

        return Ok(events);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<List<CalendarEventResponse>>> GetUpcoming()
    {
        var userId = GetUserId();
        var events = await _calendarEventService.GetUpcomingAsync(userId);

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CalendarEventResponse>> GetById(int id)
    {
        var userId = GetUserId();
        var calendarEvent = await _calendarEventService.GetByIdAsync(userId, id);

        if (calendarEvent is null)
        {
            return NotFound();
        }

        return Ok(calendarEvent);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CalendarEventResponse>> Update(
        int id,
        [FromBody] UpdateCalendarEventRequest request)
    {
        var userId = GetUserId();

        try
        {
            var updatedEvent = await _calendarEventService.UpdateAsync(userId, id, request);

            if (updatedEvent is null)
            {
                return NotFound();
            }

            return Ok(updatedEvent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var deleted = await _calendarEventService.DeleteAsync(userId, id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
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
