namespace JobApplicationManager.API.Features.Jobs.DTOs;

public class JobSearchRequest
{
    public string? Q { get; set; }
    public bool? Remote { get; set; }
    public string? Sort { get; set; } = "pubdate-desc";
    public string? PublishedAfter { get; set; }
    public int Offset { get; set; } = 0;
    public int Limit { get; set; } = 10;
}
