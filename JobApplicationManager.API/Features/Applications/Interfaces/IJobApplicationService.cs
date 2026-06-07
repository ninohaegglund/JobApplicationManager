using JobApplicationManager.API.Features.Applications.DTOs;

namespace JobApplicationManager.API.Features.Applications.Interfaces;

public interface IJobApplicationService
{
    Task<JobApplicationResponse> CreateAsync(Guid userId, CreateJobApplicationRequest request);
    Task<List<JobApplicationResponse>> GetAllForUserAsync(Guid userId, string? search = null);
    Task<JobApplicationResponse?> GetByIdAsync(Guid userId, int id);
    Task<JobApplicationsExportFile> ExportToExcelAsync(Guid userId);
    Task<bool> UpdateStatusAsync(Guid userId, int id, UpdateApplicationStatusRequest request);
    Task<bool> DeleteAsync(Guid userId, int id);
}
