using JobApplicationManager.API.Data.Context;
using JobApplicationManager.API.Data.Entities;
using JobApplicationManager.API.Features.Applications.DTOs;
using JobApplicationManager.API.Features.Applications.Interfaces;
using JobApplicationManager.API.Features.Exports;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManager.API.Features.Applications.Services;

public class JobApplicationService : IJobApplicationService
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly DataContext _context;

    public JobApplicationService(DataContext context)
    {
        _context = context;
    }

    public async Task<JobApplicationResponse> CreateAsync(Guid userId, CreateJobApplicationRequest request)
    {
        var entity = new JobApplication
        {
            UserId = userId,
            CompanyName = request.CompanyName,
            RoleTitle = request.RoleTitle,
            Notes = request.Notes,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.JobApplications.Add(entity);
        await _context.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<List<JobApplicationResponse>> GetAllForUserAsync(Guid userId)
    {
        var applications = await _context.JobApplications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return applications
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<JobApplicationResponse?> GetByIdAsync(Guid userId, int id)
    {
        var application = await _context.JobApplications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (application is null)
        {
            return null;
        }

        return MapToResponse(application);
    }

    public async Task<JobApplicationsExportFile> ExportToExcelAsync(Guid userId)
    {
        var applications = await _context.JobApplications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new JobApplicationResponse
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                RoleTitle = x.RoleTitle,
                Status = x.Status,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return new JobApplicationsExportFile
        {
            Content = JobApplicationsExcelExporter.CreateWorkbook(applications),
            ContentType = ExcelContentType,
            FileName = $"job-applications-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        };
    }

    public async Task<bool> UpdateStatusAsync(Guid userId, int id, UpdateApplicationStatusRequest request)
    {
        var application = await _context.JobApplications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (application is null)
        {
            return false;
        }

        application.Status = request.Status;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid userId, int id)
    {
        var application = await _context.JobApplications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (application is null)
        {
            return false;
        }

        _context.JobApplications.Remove(application);
        await _context.SaveChangesAsync();

        return true;
    }

    private static JobApplicationResponse MapToResponse(JobApplication entity)
    {
        return new JobApplicationResponse
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            RoleTitle = entity.RoleTitle,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
