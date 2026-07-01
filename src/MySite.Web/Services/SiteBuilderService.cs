using System.Text.Json.Nodes;
using CIMC.Data.Entities;
using CIMC.EntityFramework;
using CIMC.Helper;
using CIMC.WebSite.Models;
using Microsoft.EntityFrameworkCore;

namespace CIMC.WebSite.Services;

public interface ISiteBuilderService
{
    Task<List<SitePageDto>> GetPagesAsync();
    Task<SitePageDto> GetPageAsync(string pageKey);
    Task SavePageAsync(SitePageDto dto, string? userName);
    List<ComponentTemplateDto> GetTemplates();
}

public class SiteBuilderService : ISiteBuilderService
{
    private readonly AppDbContext _dbContext;

    public SiteBuilderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SitePageDto>> GetPagesAsync()
    {
        var pages = await _dbContext.SitePages.Include(p => p.Sections).Where(p => !p.IsDeleted).OrderBy(p => p.PageKey).ToListAsync();
        return pages.Select(ToDto).ToList();
    }

    public async Task<SitePageDto> GetPageAsync(string pageKey)
    {
        pageKey = Normalize(pageKey);
        var page = await _dbContext.SitePages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.PageKey == pageKey && !p.IsDeleted)
                   ?? await _dbContext.SitePages.Include(p => p.Sections).FirstAsync(p => p.PageKey == "home");
        return ToDto(page);
    }

    public async Task SavePageAsync(SitePageDto dto, string? userName)
    {
        dto.PageKey = Normalize(dto.PageKey);
        var page = await _dbContext.SitePages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.PageKey == dto.PageKey);
        if (page == null)
        {
            page = new SitePage { PageKey = dto.PageKey, CreatedBy = userName, CreationTime = DateTime.Now };
            _dbContext.SitePages.Add(page);
        }

        page.Title = string.IsNullOrWhiteSpace(dto.Title) ? dto.PageKey : dto.Title;
        page.Description = dto.Description;
        page.Keywords = dto.Keywords;
        page.UpdateTime = DateTime.Now;
        page.UpdatedBy = userName;
        page.IsDeleted = false;

        _dbContext.SiteSections.RemoveRange(page.Sections);
        page.Sections = dto.Sections.Select((section, index) => new SiteSection
        {
            SectionKey = string.IsNullOrWhiteSpace(section.Id) ? Guid.NewGuid().ToString("N") : section.Id,
            Component = string.IsNullOrWhiteSpace(section.Component) ? "rich-text" : section.Component,
            Name = string.IsNullOrWhiteSpace(section.Name) ? section.Component : section.Name,
            Title = section.Title,
            SubTitle = section.SubTitle,
            LinkText = section.LinkText,
            LinkUrl = section.LinkUrl,
            ImagesJson = JsonHelper.Serialize(section.Images ?? new List<string>()),
            SettingsJson = section.Settings?.ToJsonString() ?? "{}",
            Sort = (index + 1) * 10,
            IsEnabled = section.IsEnabled,
            CreatedBy = userName,
            CreationTime = DateTime.Now
        }).ToList();

        await _dbContext.SaveChangesAsync();
    }

    public List<ComponentTemplateDto> GetTemplates()
    {
        return new List<ComponentTemplateDto>
        {
            Template("hero", "首屏 Banner", "营销组件", "首页首屏或内页横幅", new SiteSectionDto { Component = "hero", Name = "首屏 Banner", Title = "通用型企业门户网站", SubTitle = "后台拖拽配置页面模块，快速生成 PC 官网。", LinkText = "了解更多", LinkUrl = "/about", Images = new List<string>{"/img/default-hero.svg"}, Settings = Obj("{\"height\":\"640px\"}") }),
            Template("rich-text", "富文本内容", "基础组件", "普通文本、HTML 内容、说明文案", new SiteSectionDto { Component = "rich-text", Name = "富文本内容", Title = "内容标题", SubTitle = "内容副标题", Settings = Obj("{\"html\":\"<p>请填写内容。</p>\"}") }),
            Template("feature-grid", "能力宫格", "展示组件", "展示业务能力、产品优势、解决方案", new SiteSectionDto { Component = "feature-grid", Name = "能力宫格", Title = "核心能力", SubTitle = "可配置多项能力卡片", Settings = Obj("{\"columns\":3,\"items\":[{\"title\":\"可视化配置\",\"description\":\"拖拽组合页面模块。\",\"icon\":\"01\"}]}") }),
            Template("image-text", "图文介绍", "展示组件", "左图右文或左文右图", new SiteSectionDto { Component = "image-text", Name = "图文介绍", Title = "图文介绍标题", SubTitle = "填写简要说明", Images = new List<string>{"/img/default-image.svg"}, Settings = Obj("{\"reverse\":false,\"description\":\"这里填写图文模块正文内容。\"}") }),
            Template("stats", "数据指标", "展示组件", "客户数量、项目数量、行业经验", new SiteSectionDto { Component = "stats", Name = "数据指标", Title = "关键数据", Settings = Obj("{\"items\":[{\"value\":\"100+\",\"label\":\"服务客户\"},{\"value\":\"50+\",\"label\":\"交付项目\"}]}") }),
            Template("cta", "行动号召", "营销组件", "联系咨询、提交需求、了解方案", new SiteSectionDto { Component = "cta", Name = "行动号召", Title = "开始配置你的网站", SubTitle = "登录后台即可拖拽配置。", LinkText = "进入后台", LinkUrl = "/Admin", Settings = Obj("{\"background\":\"#0b2b5c\"}") })
        };
    }

    private static ComponentTemplateDto Template(string key, string name, string category, string description, SiteSectionDto section)
    {
        section.Component = key;
        return new ComponentTemplateDto { Key = key, Name = name, Category = category, Description = description, DefaultSection = section };
    }

    private static SitePageDto ToDto(SitePage page)
    {
        return new SitePageDto
        {
            PageKey = page.PageKey,
            Title = page.Title,
            Description = page.Description,
            Keywords = page.Keywords,
            Sections = page.Sections.Where(p => !p.IsDeleted).OrderBy(p => p.Sort).Select(p => new SiteSectionDto
            {
                Id = p.SectionKey,
                Component = p.Component,
                Name = p.Name,
                Title = p.Title,
                SubTitle = p.SubTitle,
                LinkText = p.LinkText,
                LinkUrl = p.LinkUrl,
                Images = JsonHelper.Deserialize<List<string>>(p.ImagesJson) ?? new List<string>(),
                Sort = p.Sort,
                IsEnabled = p.IsEnabled,
                Settings = Obj(p.SettingsJson)
            }).ToList()
        };
    }

    private static JsonObject Obj(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
        }
        catch
        {
            return new JsonObject();
        }
    }

    private static string Normalize(string? pageKey)
    {
        pageKey = string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey.Trim().Trim('/').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(pageKey) ? "home" : pageKey;
    }
}
