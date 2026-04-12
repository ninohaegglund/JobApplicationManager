namespace JobApplicationManager.API.Data.Entities;

public class UserProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }   // Id från IdentityService

    public string PhoneNumber { get; set; } = string.Empty;
    public string? AddressLine { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int? BirthYear { get; set; }
    public string? LegalGender { get; set; }

    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
