using JobApplicationManager.API.Data.Context;
using JobApplicationManager.API.Data.Entities;
using JobApplicationManager.API.Data.Enums;
using JobApplicationManager.API.Features.ApplicationEmails.DTOs;
using JobApplicationManager.API.Features.ApplicationEmails.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManager.API.Features.ApplicationEmails.Services;

public class ApplicationEmailService : IApplicationEmailService
{
    private readonly DataContext _context;

    public ApplicationEmailService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicationEmailDto>?> GetAllForJobApplicationAsync(Guid userId, int jobApplicationId)
    {
        var ownsApplication = await _context.JobApplications
            .AnyAsync(x => x.Id == jobApplicationId && x.UserId == userId);

        if (!ownsApplication)
        {
            return null;
        }

        var emails = await _context.ApplicationEmails
            .AsNoTracking()
            .Where(x => x.JobApplicationId == jobApplicationId)
            .OrderByDescending(x => x.ReceivedAt)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        return emails
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ApplicationEmailDto?> GetByIdAsync(Guid userId, int id)
    {
        var email = await _context.ApplicationEmails
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.JobApplication.UserId == userId);

        if (email is null)
        {
            return null;
        }

        return MapToDto(email);
    }

    public async Task<ApplicationEmailDto?> CreateAsync(
        Guid userId,
        int jobApplicationId,
        CreateApplicationEmailDto request)
    {
        var ownsApplication = await _context.JobApplications
            .AnyAsync(x => x.Id == jobApplicationId && x.UserId == userId);

        if (!ownsApplication)
        {
            return null;
        }

        ValidateRequest(request.Subject, request.Sender, request.Body, request.ReceivedAt, request.Type);

        var email = new ApplicationEmail
        {
            JobApplicationId = jobApplicationId,
            Subject = request.Subject.Trim(),
            Sender = request.Sender.Trim(),
            Body = request.Body,
            ReceivedAt = request.ReceivedAt!.Value,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApplicationEmails.Add(email);
        await _context.SaveChangesAsync();

        return MapToDto(email);
    }

    public async Task<ApplicationEmailDto?> UpdateAsync(Guid userId, int id, UpdateApplicationEmailDto request)
    {
        var email = await _context.ApplicationEmails
            .FirstOrDefaultAsync(x => x.Id == id && x.JobApplication.UserId == userId);

        if (email is null)
        {
            return null;
        }

        ValidateRequest(request.Subject, request.Sender, request.Body, request.ReceivedAt, request.Type);

        email.Subject = request.Subject.Trim();
        email.Sender = request.Sender.Trim();
        email.Body = request.Body;
        email.ReceivedAt = request.ReceivedAt!.Value;
        email.Type = request.Type;

        await _context.SaveChangesAsync();

        return MapToDto(email);
    }

    public async Task<bool> DeleteAsync(Guid userId, int id)
    {
        var email = await _context.ApplicationEmails
            .FirstOrDefaultAsync(x => x.Id == id && x.JobApplication.UserId == userId);

        if (email is null)
        {
            return false;
        }

        _context.ApplicationEmails.Remove(email);
        await _context.SaveChangesAsync();

        return true;
    }

    private static void ValidateRequest(
        string subject,
        string sender,
        string body,
        DateTime? receivedAt,
        EmailType type)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject is required.");
        }

        if (string.IsNullOrWhiteSpace(sender))
        {
            throw new ArgumentException("Sender is required.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.");
        }

        if (!receivedAt.HasValue)
        {
            throw new ArgumentException("Received date is required.");
        }

        if (!Enum.IsDefined(typeof(EmailType), type))
        {
            throw new ArgumentException("Email type is invalid.");
        }
    }

    private static ApplicationEmailDto MapToDto(ApplicationEmail entity)
    {
        return new ApplicationEmailDto
        {
            Id = entity.Id,
            JobApplicationId = entity.JobApplicationId,
            Subject = entity.Subject,
            Sender = entity.Sender,
            Body = entity.Body,
            ReceivedAt = entity.ReceivedAt,
            Type = entity.Type,
            CreatedAt = entity.CreatedAt
        };
    }
}
