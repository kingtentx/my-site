using MySite.Web.Models;

namespace MySite.Web.Services;

public interface ISitePageStore
{
    Task EnsureSeedDataAsync();

    Task<IReadOnlyList<SitePage>> GetAllPagesAsync();

    Task<SitePage> GetPageAsync(string pageKey);

    Task SavePageAsync(SitePage page);

    IReadOnlyList<ComponentTemplate> GetTemplates();
}
