using System.Security.Claims;
using JobApplicationManager.API.Features.Applications.DTOs;
using JobApplicationManager.API.Features.Applications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManager.API.Features.Applications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public JobApplicationsController(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    [HttpPost]
    public async Task<ActionResult<JobApplicationResponse>> Create([FromBody] CreateJobApplicationRequest request)
    {
        var userId = GetUserId();

        var createdApplication = await _jobApplicationService.CreateAsync(userId, request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdApplication.Id },
            createdApplication);
    }

    [HttpGet]
    public async Task<ActionResult<List<JobApplicationResponse>>> GetAll()
    {
        var userId = GetUserId();

        var applications = await _jobApplicationService.GetAllForUserAsync(userId);

        return Ok(applications);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel()
    {
        var userId = GetUserId();

        var export = await _jobApplicationService.ExportToExcelAsync(userId);

        return File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobApplicationResponse>> GetById(int id)
    {
        var userId = GetUserId();

        var application = await _jobApplicationService.GetByIdAsync(userId, id);

        if (application is null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusRequest request)
    {
        var userId = GetUserId();

        var updated = await _jobApplicationService.UpdateStatusAsync(userId, id, request);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var deleted = await _jobApplicationService.DeleteAsync(userId, id);
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
