using CIMC.Data;
using CIMC.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySite.Web
{
    /// <summary>
    /// 复用 ContentProductCategory 表作为文章、产品、招聘的统一内容分类树。
    /// </summary>
    public static class ContentCategoryHelper
    {
        public const string ArticleRoot = "文章";
        public const string ProductRoot = "产品";
        public const string JobRoot = "招聘";

        private static readonly string[] RootNames = { ArticleRoot, ProductRoot, JobRoot };

        public static void EnsureRoots(IRepository<ContentProductCategory> repository)
        {
            var roots = repository.GetList(p => !p.IsDelete && p.Pid == 0, p => p.Sort, true);
            var articleRoot = EnsureRoot(repository, roots, ArticleRoot, 1);
            var productRoot = EnsureRoot(repository, roots, ProductRoot, 2);
            var jobRoot = EnsureRoot(repository, roots, JobRoot, 3);

            // 兼容旧的“产品分类”：原顶级产品分类统一挂到“产品”大类下。
            roots = repository.GetList(p => !p.IsDelete && p.Pid == 0, p => p.Sort, true);
            foreach (var legacyRoot in roots.Where(p => !RootNames.Contains(p.Name)))
            {
                legacyRoot.Pid = productRoot.Id;
                legacyRoot.UpdateBy = "system";
                legacyRoot.UpdateTime = DateTime.Now;
                repository.Update(legacyRoot);
            }

            EnsureDefaultChildren(repository, articleRoot.Id, new[]
            {
                "公司新闻", "行业资讯", "技术文章", "通知公告"
            });
            EnsureDefaultChildren(repository, jobRoot.Id, new[]
            {
                "技术研发", "产品与设计", "市场与销售", "综合职能", "实习招聘"
            });

            // 新库没有原产品分类时提供一个默认分类。
            if (!repository.GetList(p => !p.IsDelete && p.Pid == productRoot.Id).Any())
            {
                AddCategory(repository, productRoot.Id, "默认产品", 1);
            }
        }

        public static int GetRootId(IRepository<ContentProductCategory> repository, string contentType)
        {
            EnsureRoots(repository);
            var name = ResolveRootName(contentType);
            return repository.GetOne(p => !p.IsDelete && p.Pid == 0 && p.Name == name)?.Id ?? 0;
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
            var all = repository.GetList(p => !p.IsDelete && (!activeOnly || p.IsActive), p => p.Sort, true);
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

        private static ContentProductCategory EnsureRoot(
            IRepository<ContentProductCategory> repository,
            IEnumerable<ContentProductCategory> currentRoots,
            string name,
            int sort)
        {
            var root = currentRoots.FirstOrDefault(p => p.Name == name);
            if (root != null) return root;

            root = new ContentProductCategory
            {
                Pid = 0,
                Name = name,
                Sort = sort,
                IsActive = true,
                IsDelete = false,
                CreationBy = "system",
                CreationTime = DateTime.Now
            };
            repository.Add(root);
            return root;
        }

        private static void EnsureDefaultChildren(
            IRepository<ContentProductCategory> repository,
            int parentId,
            IEnumerable<string> names)
        {
            var current = repository.GetList(p => !p.IsDelete && p.Pid == parentId);
            var sort = 1;
            foreach (var name in names)
            {
                if (!current.Any(p => p.Name == name)) AddCategory(repository, parentId, name, sort);
                sort++;
            }
        }

        private static void AddCategory(
            IRepository<ContentProductCategory> repository,
            int parentId,
            string name,
            int sort)
        {
            repository.Add(new ContentProductCategory
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
    }
}
