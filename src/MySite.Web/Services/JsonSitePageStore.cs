using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using MySite.Web.Models;

namespace MySite.Web.Services;

public class JsonSitePageStore : ISitePageStore
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonSitePageStore(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    private string StorePath
    {
        get
        {
            var fileName = _configuration["SiteStore:FileName"] ?? "site-pages.json";
            return Path.Combine(_environment.ContentRootPath, "App_Data", fileName);
        }
    }

    public async Task EnsureSeedDataAsync()
    {
        if (File.Exists(StorePath))
        {
            return;
        }

        var pages = new List<SitePage>
        {
            CreateHomePage(),
            new()
            {
                PageKey = "about",
                Title = "关于我们",
                Description = "通用企业介绍页面",
                Sections = new List<SiteSection>
                {
                    new()
                    {
                        Component = "hero",
                        Name = "页面 Banner",
                        Title = "关于我们",
                        SubTitle = "通过后台设计器修改这段内容",
                        Images = new List<string> { "/img/default-hero.svg" },
                        Sort = 10
                    },
                    new()
                    {
                        Component = "rich-text",
                        Name = "企业介绍",
                        Title = "企业介绍",
                        Sort = 20,
                        Settings = JsonObject.Parse("{\"html\":\"<p>这里是通用企业介绍，可在后台可视化设计器中编辑。</p>\"}")!.AsObject()
                    }
                }
            }
        };

        await SaveAllAsync(pages);
    }

    public async Task<IReadOnlyList<SitePage>> GetAllPagesAsync()
    {
        var pages = await ReadAllAsync();
        return pages.OrderBy(p => p.PageKey == "home" ? 0 : 1).ThenBy(p => p.PageKey).ToList();
    }

    public async Task<SitePage> GetPageAsync(string pageKey)
    {
        pageKey = NormalizePageKey(pageKey);
        var pages = await ReadAllAsync();
        return pages.FirstOrDefault(p => NormalizePageKey(p.PageKey) == pageKey)
               ?? pages.FirstOrDefault(p => NormalizePageKey(p.PageKey) == "home")
               ?? CreateHomePage();
    }

    public async Task SavePageAsync(SitePage page)
    {
        page.PageKey = NormalizePageKey(page.PageKey);
        page.UpdatedAt = DateTime.Now;
        page.Sections ??= new List<SiteSection>();

        for (var i = 0; i < page.Sections.Count; i++)
        {
            var section = page.Sections[i];
            section.Id = string.IsNullOrWhiteSpace(section.Id) ? Guid.NewGuid().ToString("N") : section.Id;
            section.Component = string.IsNullOrWhiteSpace(section.Component) ? "rich-text" : section.Component;
            section.Name = string.IsNullOrWhiteSpace(section.Name) ? section.Component : section.Name;
            section.Sort = (i + 1) * 10;
            section.Settings ??= new JsonObject();
        }

        var pages = await ReadAllAsync();
        var index = pages.FindIndex(p => NormalizePageKey(p.PageKey) == page.PageKey);
        if (index >= 0)
        {
            pages[index] = page;
        }
        else
        {
            pages.Add(page);
        }

        await SaveAllAsync(pages);
    }

    public IReadOnlyList<ComponentTemplate> GetTemplates()
    {
        return new List<ComponentTemplate>
        {
            new()
            {
                Key = "hero",
                Name = "首屏 Banner",
                Category = "营销组件",
                Description = "适合首页首屏或内页横幅，支持背景图、标题、副标题和按钮。",
                DefaultSection = new SiteSection
                {
                    Component = "hero",
                    Name = "首屏 Banner",
                    Title = "打造通用型企业门户",
                    SubTitle = "通过后台拖拽配置页面模块，快速搭建 PC 门户网站。",
                    LinkText = "了解更多",
                    LinkUrl = "/about",
                    Images = new List<string> { "/img/default-hero.svg" },
                    Settings = JsonObject.Parse("{\"height\":\"640px\",\"align\":\"left\"}")!.AsObject()
                }
            },
            new()
            {
                Key = "rich-text",
                Name = "富文本内容",
                Category = "基础组件",
                Description = "用于普通文本、HTML 内容、说明文案。",
                DefaultSection = new SiteSection
                {
                    Component = "rich-text",
                    Name = "富文本内容",
                    Title = "内容标题",
                    SubTitle = "内容副标题",
                    Settings = JsonObject.Parse("{\"html\":\"<p>请在这里填写页面内容。</p>\"}")!.AsObject()
                }
            },
            new()
            {
                Key = "feature-grid",
                Name = "能力宫格",
                Category = "展示组件",
                Description = "适合展示业务能力、产品优势、解决方案。",
                DefaultSection = new SiteSection
                {
                    Component = "feature-grid",
                    Name = "能力宫格",
                    Title = "核心能力",
                    SubTitle = "可按需配置多项能力卡片",
                    Settings = JsonObject.Parse("{\"columns\":3,\"items\":[{\"title\":\"可视化配置\",\"description\":\"拖拽组合页面模块。\",\"icon\":\"01\"},{\"title\":\"组件化渲染\",\"description\":\"复用统一组件模板。\",\"icon\":\"02\"},{\"title\":\"快速发布\",\"description\":\"保存后前台立即生效。\",\"icon\":\"03\"}]}")!.AsObject()
                }
            },
            new()
            {
                Key = "image-text",
                Name = "图文介绍",
                Category = "展示组件",
                Description = "支持左图右文、左文右图的企业介绍或产品说明。",
                DefaultSection = new SiteSection
                {
                    Component = "image-text",
                    Name = "图文介绍",
                    Title = "图文介绍标题",
                    SubTitle = "填写简要说明",
                    LinkText = "查看详情",
                    LinkUrl = "/about",
                    Images = new List<string> { "/img/default-image.svg" },
                    Settings = JsonObject.Parse("{\"reverse\":false,\"description\":\"这里填写图文模块正文内容。\"}")!.AsObject()
                }
            },
            new()
            {
                Key = "stats",
                Name = "数据指标",
                Category = "展示组件",
                Description = "用于展示客户数量、交付项目、行业经验等数字。",
                DefaultSection = new SiteSection
                {
                    Component = "stats",
                    Name = "数据指标",
                    Title = "关键数据",
                    Settings = JsonObject.Parse("{\"items\":[{\"value\":\"100+\",\"label\":\"服务客户\"},{\"value\":\"50+\",\"label\":\"交付项目\"},{\"value\":\"10年+\",\"label\":\"行业经验\"}]}")!.AsObject()
                }
            },
            new()
            {
                Key = "cta",
                Name = "行动号召",
                Category = "营销组件",
                Description = "用于联系我们、提交需求、了解方案等转化入口。",
                DefaultSection = new SiteSection
                {
                    Component = "cta",
                    Name = "行动号召",
                    Title = "准备开始搭建官网吗？",
                    SubTitle = "通过后台配置即可快速生成页面。",
                    LinkText = "联系我们",
                    LinkUrl = "/about",
                    Settings = JsonObject.Parse("{\"background\":\"#0b2b5c\"}")!.AsObject()
                }
            }
        };
    }

    private async Task<List<SitePage>> ReadAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(StorePath))
            {
                return new List<SitePage>();
            }

            var json = await File.ReadAllTextAsync(StorePath);
            return JsonSerializer.Deserialize<List<SitePage>>(json, _jsonOptions) ?? new List<SitePage>();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveAllAsync(List<SitePage> pages)
    {
        await _lock.WaitAsync();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            var json = JsonSerializer.Serialize(pages, _jsonOptions);
            await File.WriteAllTextAsync(StorePath, json);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string NormalizePageKey(string? pageKey)
    {
        pageKey = string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey.Trim().Trim('/').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey;
    }

    private static SitePage CreateHomePage()
    {
        return new SitePage
        {
            PageKey = "home",
            Title = "通用门户网站",
            Description = "通用型 PC 门户网站，可通过后台可视化拖拽配置。",
            Keywords = "门户网站,可视化建站,拖拽配置",
            Sections = new List<SiteSection>
            {
                new()
                {
                    Component = "hero",
                    Name = "首页 Banner",
                    Title = "通用型企业门户网站",
                    SubTitle = "后台可视化拖拽配置页面，快速生成 PC 官网。",
                    LinkText = "进入后台配置",
                    LinkUrl = "/Admin",
                    Images = new List<string> { "/img/default-hero.svg" },
                    Sort = 10,
                    Settings = JsonObject.Parse("{\"height\":\"640px\",\"align\":\"left\"}")!.AsObject()
                },
                new()
                {
                    Component = "feature-grid",
                    Name = "核心能力",
                    Title = "核心能力",
                    SubTitle = "从固定门户升级为通用可配置门户",
                    Sort = 20,
                    Settings = JsonObject.Parse("{\"columns\":3,\"items\":[{\"title\":\"组件化页面\",\"description\":\"页面由 Banner、图文、宫格、指标、CTA 等组件组合。\",\"icon\":\"01\"},{\"title\":\"可视化拖拽\",\"description\":\"后台拖拽排序，右侧配置内容与样式。\",\"icon\":\"02\"},{\"title\":\"通用扩展\",\"description\":\"后续可接入数据库、主题模板、发布审核。\",\"icon\":\"03\"}]}")!.AsObject()
                },
                new()
                {
                    Component = "stats",
                    Name = "数据指标",
                    Title = "系统特性",
                    Sort = 30,
                    Settings = JsonObject.Parse("{\"items\":[{\"value\":\"6+\",\"label\":\"内置组件\"},{\"value\":\"0\",\"label\":\"数据库依赖\"},{\"value\":\"100%\",\"label\":\"源码可控\"}]}")!.AsObject()
                },
                new()
                {
                    Component = "cta",
                    Name = "行动号召",
                    Title = "开始配置你的网站",
                    SubTitle = "登录后台，拖拽组件并保存后即可看到前台变化。",
                    LinkText = "进入后台",
                    LinkUrl = "/Admin",
                    Sort = 40,
                    Settings = JsonObject.Parse("{\"background\":\"#0b2b5c\"}")!.AsObject()
                }
            }
        };
    }
}
