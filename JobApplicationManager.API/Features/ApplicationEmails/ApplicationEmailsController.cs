using System.Security.Claims;
using JobApplicationManager.API.Features.ApplicationEmails.DTOs;
using JobApplicationManager.API.Features.ApplicationEmails.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManager.API.Features.ApplicationEmails;

[ApiController]
[Route("api")]
[Authorize]
public class ApplicationEmailsController : ControllerBase
{
    private readonly IApplicationEmailService _applicationEmailService;

    public ApplicationEmailsController(IApplicationEmailService applicationEmailService)
    {
        _applicationEmailService = applicationEmailService;
    }

    [HttpGet("jobapplications/{jobApplicationId:int}/emails")]
    public async Task<ActionResult<List<ApplicationEmailDto>>> GetForJobApplication(int jobApplicationId)
    {
        var userId = GetUserId();

        var emails = await _applicationEmailService.GetAllForJobApplicationAsync(userId, jobApplicationId);

        if (emails is null)
        {
            return NotFound();
        }

        return Ok(emails);
    }

    [HttpGet("applicationemails/{id:int}")]
    public async Task<ActionResult<ApplicationEmailDto>> GetById(int id)
    {
        var userId = GetUserId();

        var email = await _applicationEmailService.GetByIdAsync(userId, id);

        if (email is null)
        {
            return NotFound();
        }

        return Ok(email);
    }

    [HttpPost("jobapplications/{jobApplicationId:int}/emails")]
    public async Task<ActionResult<ApplicationEmailDto>> Create(
        int jobApplicationId,
        [FromBody] CreateApplicationEmailDto request)
    {
        var userId = GetUserId();

        try
        {
            var createdEmail = await _applicationEmailService.CreateAsync(userId, jobApplicationId, request);

            if (createdEmail is null)
            {
                return NotFound();
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdEmail.Id },
                createdEmail);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("applicationemails/{id:int}")]
    public async Task<ActionResult<ApplicationEmailDto>> Update(
        int id,
        [FromBody] UpdateApplicationEmailDto request)
    {
        var userId = GetUserId();

        try
        {
            var updatedEmail = await _applicationEmailService.UpdateAsync(userId, id, request);

            if (updatedEmail is null)
            {
                return NotFound();
            }

            return Ok(updatedEmail);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("applicationemails/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted = await _applicationEmailService.DeleteAsync(userId, id);

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
