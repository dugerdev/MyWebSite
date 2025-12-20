using MyWebSite.Core.Entities;

namespace MyWebSite.Web.Models;

public class ResumeViewModel
{
    public IEnumerable<ResumeItem> Experience { get; set; } = Enumerable.Empty<ResumeItem>();
    public IEnumerable<ResumeItem> Educations { get; set; } = Enumerable.Empty<ResumeItem>();
    public IEnumerable<Skill> ProfessionalSkills { get; set; } = Enumerable.Empty<Skill>();
    public IEnumerable<Skill> Languages { get; set; } = Enumerable.Empty<Skill>();
}
