using Microsoft.AspNetCore.Http;

namespace JobApplicationManager.API.Features.CvDocuments.DTOs;

public class UploadCvDocumentRequest
{
    public required IFormFile File { get; set; }
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
}
