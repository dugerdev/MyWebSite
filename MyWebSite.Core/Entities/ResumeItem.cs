using MyWebSite.Core.Enums;

namespace MyWebSite.Core.Entities;

public class ResumeItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string CompanyOrInstitution { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } 
    public ResumeItemType Type { get; set; }
}
