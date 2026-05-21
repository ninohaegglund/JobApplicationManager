using System.ComponentModel.DataAnnotations;
using JobApplicationManager.API.Data.Enums;

namespace JobApplicationManager.API.Features.ApplicationEmails.DTOs;

public class UpdateApplicationEmailDto
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Sender { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public DateTime? ReceivedAt { get; set; }

    public EmailType Type { get; set; } = EmailType.General;
}
