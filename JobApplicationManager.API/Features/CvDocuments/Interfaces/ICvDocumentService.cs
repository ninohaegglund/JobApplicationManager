using JobApplicationManager.API.Features.CvDocuments.DTOs;

namespace JobApplicationManager.API.Features.CvDocuments.Interfaces;

public interface ICvDocumentService
{
    Task<CvDocumentResponse> UploadAsync(Guid userId, UploadCvDocumentRequest request);
    Task<List<CvDocumentResponse>> GetAllForUserAsync(Guid userId);
    Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadAsync(Guid userId, Guid id);
    Task<(byte[] FileBytes, string ContentType, string FileName)?> PreviewAsync(Guid userId, Guid id);
    Task<bool> SetDefaultAsync(Guid userId, Guid id);
    Task<bool> DeleteAsync(Guid userId, Guid id);
}
