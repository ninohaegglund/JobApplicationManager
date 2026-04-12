using System.ComponentModel.DataAnnotations;

namespace JobApplicationManager.API.Features.Applications.DTOs;

public class UpdateApplicationStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
