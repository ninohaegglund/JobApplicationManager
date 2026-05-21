using JobApplicationManager.API.Features.ApplicationEmails.DTOs;

namespace JobApplicationManager.API.Features.ApplicationEmails.Interfaces;

public interface IApplicationEmailService
{
    Task<List<ApplicationEmailDto>?> GetAllForJobApplicationAsync(Guid userId, int jobApplicationId);
    Task<ApplicationEmailDto?> GetByIdAsync(Guid userId, int id);
    Task<ApplicationEmailDto?> CreateAsync(Guid userId, int jobApplicationId, CreateApplicationEmailDto request);
    Task<ApplicationEmailDto?> UpdateAsync(Guid userId, int id, UpdateApplicationEmailDto request);
    Task<bool> DeleteAsync(Guid userId, int id);
}
