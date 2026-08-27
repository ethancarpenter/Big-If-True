namespace CampaignApp.Application.DTOs;

public class SearchResultDto
{
    public SearchResultType Type { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>e.g. the City name for a Location result; null where there's no useful parent to show.</summary>
    public string? ParentContext { get; set; }

    public string Url { get; set; } = string.Empty;
}
