using MyWebSite.Core.Entities;

namespace MyWebSite.Web.Models;

public class HomeIndexViewModel
{
    public IEnumerable<Project> FeaturedProjects { get; set; } = Enumerable.Empty<Project>();
    public AboutMe? AboutMe { get; set; }
}

