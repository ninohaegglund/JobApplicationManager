using JobApplicationManager.API.Data.Enums;

namespace JobApplicationManager.API.Features.ApplicationEmails.DTOs;

public class ApplicationEmailDto
{
    public int Id { get; set; }
    public int JobApplicationId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public EmailType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
