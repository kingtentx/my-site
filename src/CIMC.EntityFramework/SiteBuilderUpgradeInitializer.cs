using CIMC.EntityFrameworkCore;

namespace CIMC.Data
{
    /// <summary>
    /// 启动初始化入口。
    ///
    /// 历史 Site Builder 数据的破坏性清理由 scripts/site_builder_reset_v3.sql
    /// 手工执行；应用启动时只补齐运行所需的基础数据，不再自动删除页面、导航、
    /// Footer 或业务内容，避免每次启动修改用户数据。
    /// </summary>
    public class SiteBuilderUpgradeInitializer
    {
        public void Apply(AppDbContext context)
        {
            new DataInitializer().Create(context);
        }
    }
}
