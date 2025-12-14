namespace MyWebSite.Core.Entities;

public class Project : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Technologies { get; set; }
    public string? GitHubUrl { get; set; }
    public string? LiveUrl { get; set; }
    public bool IsFeatured { get; set; }
}
