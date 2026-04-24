using System.ComponentModel.DataAnnotations;

namespace JobApplicationManager.API.Features.Applications.DTOs;

public class CreateJobApplicationRequest
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string RoleTitle { get; set; } = string.Empty;
    [Required]
    public string Status { get; set; } = "Draft";
    [MaxLength(2000)]
    public string? Notes { get; set; }
}
