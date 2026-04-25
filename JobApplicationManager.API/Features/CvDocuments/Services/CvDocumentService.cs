using JobApplicationManager.API.Data.Context;
using JobApplicationManager.API.Data.Entities;
using JobApplicationManager.API.Features.CvDocuments.DTOs;
using JobApplicationManager.API.Features.CvDocuments.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManager.API.Features.CvDocuments.Services;

public class CvDocumentService : ICvDocumentService
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _environment;

    public CvDocumentService(DataContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<CvDocumentResponse> UploadAsync(Guid userId, UploadCvDocumentRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            throw new ArgumentException("No file was uploaded.");
        }

        var allowedContentTypes = new[]
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

        var allowedExtensions = new[] { ".pdf", ".docx" };

        if (!allowedContentTypes.Contains(request.File.ContentType) || !allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("Only PDF and DOCX files are allowed.");
        }

        var uploadsRoot = Path.Combine(_environment.ContentRootPath, "Storage", "CvDocuments", userId.ToString());

        if (!Directory.Exists(uploadsRoot))
        {
            Directory.CreateDirectory(uploadsRoot);
        }

        var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        var entity = new CvDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = string.IsNullOrWhiteSpace(request.Name)
                ? Path.GetFileNameWithoutExtension(request.File.FileName)
                : request.Name.Trim(),
            OriginalFileName = request.File.FileName,
            StoredFileName = storedFileName,
            FilePath = fullPath,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            IsDefault = request.IsDefault || !await _context.CvDocuments.AnyAsync(x => x.UserId == userId),
            UploadedAt = DateTime.UtcNow
        };

        _context.CvDocuments.Add(entity);
        await _context.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<List<CvDocumentResponse>> GetAllForUserAsync(Guid userId)
    {
        var documents = await _context.CvDocuments
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.UploadedAt)
            .ToListAsync();

        return documents.Select(MapToResponse).ToList();
    }

    public async Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadAsync(Guid userId, Guid id)
    {
        var document = await _context.CvDocuments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (document is null || !File.Exists(document.FilePath))
        {
            return null;
        }

        var fileBytes = await File.ReadAllBytesAsync(document.FilePath);

        return (fileBytes, document.ContentType, document.OriginalFileName);
    }

    public async Task<(byte[] FileBytes, string ContentType, string FileName)?> PreviewAsync(Guid userId, Guid id)
    {
        var document = await _context.CvDocuments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (document is null || !File.Exists(document.FilePath))
        {
            return null;
        }

        if (document.ContentType != "application/pdf")
        {
            return null;
        }

        var fileBytes = await File.ReadAllBytesAsync(document.FilePath);

        return (fileBytes, document.ContentType, document.OriginalFileName);
    }

    public async Task<bool> SetDefaultAsync(Guid userId, Guid id)
    {
        var document = await _context.CvDocuments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (document is null)
        {
            return false;
        }

        var userDocuments = await _context.CvDocuments
            .Where(x => x.UserId == userId && x.IsDefault)
            .ToListAsync();

        foreach (var item in userDocuments)
        {
            item.IsDefault = false;
            item.UpdatedAt = DateTime.UtcNow;
        }

        document.IsDefault = true;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var document = await _context.CvDocuments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (document is null)
        {
            return false;
        }

        _context.CvDocuments.Remove(document);
        await _context.SaveChangesAsync();

        if (File.Exists(document.FilePath))
        {
            File.Delete(document.FilePath);
        }

        var remainingDocuments = await _context.CvDocuments
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();

        if (!remainingDocuments.Any(x => x.IsDefault) && remainingDocuments.Count > 0)
        {
            remainingDocuments[0].IsDefault = true;
            remainingDocuments[0].UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    private static CvDocumentResponse MapToResponse(CvDocument entity)
    {
        return new CvDocumentResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            OriginalFileName = entity.OriginalFileName,
            ContentType = entity.ContentType,
            FileSize = entity.FileSize,
            IsDefault = entity.IsDefault,
            UploadedAt = entity.UploadedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}