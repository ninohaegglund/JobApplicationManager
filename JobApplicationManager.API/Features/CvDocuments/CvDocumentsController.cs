using System.Security.Claims;
using JobApplicationManager.API.Features.CvDocuments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManager.API.Features.CvDocuments;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CvDocumentsController : ControllerBase
{
    private readonly ICvDocumentService _cvDocumentService;

    public CvDocumentsController(ICvDocumentService cvDocumentService)
    {
        _cvDocumentService = cvDocumentService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> Upload([FromForm] DTOs.UploadCvDocumentRequest request)
    {
        var userId = GetUserId();

        var result = await _cvDocumentService.UploadAsync(userId, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var userId = GetUserId();

        var result = await _cvDocumentService.GetAllForUserAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult> Download(Guid id)
    {
        var userId = GetUserId();

        var result = await _cvDocumentService.DownloadAsync(userId, id);

        if (result is null)
        {
            return NotFound();
        }

        return File(result.Value.FileBytes, result.Value.ContentType, result.Value.FileName);
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<ActionResult> Preview(Guid id)
    {
        var userId = GetUserId();

        var result = await _cvDocumentService.PreviewAsync(userId, id);

        if (result is null)
        {
            return NotFound("Preview is only available for PDF files.");
        }

        return File(result.Value.FileBytes, result.Value.ContentType);
    }

    [HttpPatch("{id:guid}/set-default")]
    public async Task<ActionResult> SetDefault(Guid id)
    {
        var userId = GetUserId();

        var success = await _cvDocumentService.SetDefaultAsync(userId, id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var success = await _cvDocumentService.DeleteAsync(userId, id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID claim is missing.");
        }

        return Guid.Parse(userIdClaim);
    }
}