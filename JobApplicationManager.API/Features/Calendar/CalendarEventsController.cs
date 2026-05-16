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
        try
        {
            var createdEvent = await _calendarEventService.CreateAsync(request);

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
        var events = await _calendarEventService.GetAllAsync();

        return Ok(events);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<List<CalendarEventResponse>>> GetUpcoming()
    {
        var events = await _calendarEventService.GetUpcomingAsync();

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CalendarEventResponse>> GetById(int id)
    {
        var calendarEvent = await _calendarEventService.GetByIdAsync(id);

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
        try
        {
            var updatedEvent = await _calendarEventService.UpdateAsync(id, request);

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
        var deleted = await _calendarEventService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
