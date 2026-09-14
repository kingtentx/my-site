using CIMC.Data;
using CIMC.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySite.Web
{
    /// <summary>
    /// 复用 ContentProductCategory 表作为文章、产品、招聘的统一内容分类树。
    ///
    /// 关于种子数据（2026-09-14 修订）：
    /// 旧实现 <c>EnsureRoots</c> 挂在每次后台/前台请求上，采用「查不到就插一条」的写法，
    /// 既没有互斥也没有唯一约束。并发请求各自查不到就各自插入一份，
    /// 导致「文章 / 产品 / 招聘」三个内置大类在 my_site 库中被写入 3 份
    /// （三份 CreationTime 仅相差 2~3 毫秒），其默认子分类也随之重复 3 份，共 25 行冗余数据。
    ///
    /// 现在的规则：
    ///   1. 种子数据只在分类表一条记录都没有时才写入，且整段加锁串行执行（<see cref="EnsureSeed"/>）；
    ///   2. 请求路径（<see cref="GetRootId"/>、<see cref="GetDescendants"/> 等）一律只读，不再写库；
    ///   3. 历史数据的兼容迁移改为启动时单独执行，且只在确实存在待迁移行时才写
    ///      （<see cref="MigrateLegacyTopLevel"/>）。
    /// </summary>
    public static class ContentCategoryHelper
    {
        public const string ArticleRoot = "文章";
        public const string ProductRoot = "产品";
        public const string JobRoot = "招聘";

        private static readonly string[] RootNames = { ArticleRoot, ProductRoot, JobRoot };

        private static readonly string[] ArticleChildren = { "公司新闻", "行业资讯", "技术文章", "通知公告" };

        private static readonly string[] JobChildren = { "技术研发", "产品与设计", "市场与销售", "综合职能", "实习招聘" };

        /// <summary>
        /// 应用启动时执行（非请求路径）。多个请求可能并发进入，因此整段加锁。
        /// </summary>
        private static readonly object SeedLock = new object();

        #region 种子数据（只在表为空时写入）

        /// <summary>
        /// 写入默认分类树。<b>仅在分类表一条记录都没有时执行</b>，否则直接返回、不写库。
        /// </summary>
        public static void EnsureSeed(IRepository<ContentProductCategory> repository)
        {
            lock (SeedLock)
            {
                // 只要表里有任何一条（未删除的）分类，就认为已经初始化过，一律不写。
                if (repository.GetList(p => true).Any()) return;

                var articleRoot = AddCategory(repository, 0, ArticleRoot, 1);
                var productRoot = AddCategory(repository, 0, ProductRoot, 2);
                AddCategory(repository, 0, JobRoot, 3);

                EnsureChildren(repository, articleRoot.Id, ArticleChildren);
                EnsureChildren(repository, productRoot.Id, new[] { "默认产品" }, 1);
            }
        }

        /// <summary>
        /// 兼容旧数据：把历史遗留的顶级产品分类统一挂到「产品」大类下。
        /// 只在确实存在「Pid=0 且名称不是内置大类」的记录时才写库，迁移一次后即为空操作。
        /// </summary>
        public static void MigrateLegacyTopLevel(IRepository<ContentProductCategory> repository)
        {
            lock (SeedLock)
            {
                var roots = repository.GetList(p => p.Pid == 0, p => p.Sort, true);
                var productRoot = roots.FirstOrDefault(p => p.Name == ProductRoot);
                if (productRoot == null) return;

                var legacyRoots = roots.Where(p => !RootNames.Contains(p.Name)).ToList();
                foreach (var legacyRoot in legacyRoots)
                {
                    legacyRoot.Pid = productRoot.Id;
                    legacyRoot.UpdateBy = "system";
                    legacyRoot.UpdateTime = DateTime.Now;
                    repository.Update(legacyRoot);
                }
            }
        }

        private static void EnsureChildren(
            IRepository<ContentProductCategory> repository,
            int parentId,
            IEnumerable<string> names,
            int startSort = 1)
        {
            var sort = startSort;
            foreach (var name in names)
            {
                AddCategory(repository, parentId, name, sort);
                sort++;
            }
        }

        private static ContentProductCategory AddCategory(
            IRepository<ContentProductCategory> repository,
            int parentId,
            string name,
            int sort)
        {
            return repository.Add(new ContentProductCategory
            {
                Pid = parentId,
                Name = name,
                Sort = sort,
                IsActive = true,
                IsDelete = false,
                CreationBy = "system",
                CreationTime = DateTime.Now
            });
        }

        #endregion

        #region 只读查询

        public static int GetRootId(IRepository<ContentProductCategory> repository, string contentType)
        {
            var name = ResolveRootName(contentType);
            return repository.GetOne(p => p.Pid == 0 && p.Name == name)?.Id ?? 0;
        }

        public static string ResolveRootName(string contentType)
        {
            return (contentType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "article" => ArticleRoot,
                "news" => ArticleRoot,
                "job" => JobRoot,
                "recruit" => JobRoot,
                _ => ProductRoot
            };
        }

        public static bool IsFixedRoot(ContentProductCategory category)
        {
            return category != null && category.Pid == 0 && RootNames.Contains(category.Name);
        }

        public static List<ContentProductCategory> GetDescendants(
            IRepository<ContentProductCategory> repository,
            int rootId,
            bool activeOnly = false)
        {
            var all = repository.GetList(p => !activeOnly || p.IsActive, p => p.Sort, true);
            var result = new List<ContentProductCategory>();
            AppendChildren(all, rootId, result);
            return result;
        }

        public static List<int> GetDescendantIds(
            IRepository<ContentProductCategory> repository,
            int rootId,
            bool includeRoot = false)
        {
            var ids = GetDescendants(repository, rootId).Select(p => p.Id).ToList();
            if (includeRoot && rootId > 0) ids.Insert(0, rootId);
            return ids;
        }

        public static string GetIndentedName(
            ContentProductCategory category,
            IReadOnlyCollection<ContentProductCategory> all,
            int rootId)
        {
            if (category == null) return string.Empty;
            var depth = 0;
            var parentId = category.Pid;
            var guard = 0;
            while (parentId > 0 && parentId != rootId && guard++ < 10)
            {
                depth++;
                parentId = all.FirstOrDefault(p => p.Id == parentId)?.Pid ?? 0;
            }
            return new string('　', depth) + category.Name;
        }

        private static void AppendChildren(
            IReadOnlyCollection<ContentProductCategory> all,
            int parentId,
            ICollection<ContentProductCategory> result)
        {
            foreach (var child in all.Where(p => p.Pid == parentId).OrderBy(p => p.Sort).ThenBy(p => p.Id))
            {
                result.Add(child);
                AppendChildren(all, child.Id, result);
            }
        }

        #endregion
    }
}
