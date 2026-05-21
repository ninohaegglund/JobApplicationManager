using JobApplicationManager.API.Data.Enums;

namespace JobApplicationManager.API.Data.Entities;

public class ApplicationEmail
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; }

    public EmailType Type { get; set; } = EmailType.General;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
