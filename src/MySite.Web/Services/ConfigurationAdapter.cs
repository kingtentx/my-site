using CIMC.EntityFramework;

namespace CIMC.WebSite.Services;

public class ConfigurationAdapter : IConfigurationLike
{
    private readonly IConfiguration _configuration;

    public ConfigurationAdapter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? Get(string key)
    {
        return _configuration[key];
    }
}
