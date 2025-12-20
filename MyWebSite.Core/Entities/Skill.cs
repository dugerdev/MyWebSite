using MyWebSite.Core.Enums;

namespace MyWebSite.Core.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public int DisplayOrder { get; set; }
}
