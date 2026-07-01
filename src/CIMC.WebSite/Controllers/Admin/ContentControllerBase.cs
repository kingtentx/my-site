using CIMC.Core.Enums;
using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    public abstract class ContentControllerBase : AdminBaseController
    {
        private readonly IRepository<Tag> _tagRepository;

        public ContentControllerBase(IRepository<Tag> tagRepository)
        {
            _tagRepository = tagRepository;
        }

        protected List<TagModel> GetTags(int? tagType = null)
        {
            var where = LambdaHelper.True<Tag>().And(p => p.IsActive);
            if (tagType.HasValue)
            {
                var typeVal = tagType.Value;
                where = where.And(p => p.TagType == typeVal);
            }

            var list = _tagRepository.GetList(where, p => p.Sort, 1, 200, true).List;
            return list.Select(p => new TagModel
            {
                Id = p.Id,
                TagName = p.TagName,
                TagType = p.TagType,
                Sort = p.Sort,
                IsActive = p.IsActive,
                CreationTime = p.CreationTime,
                CreationBy = p.CreationBy,
                UpdateTime = p.UpdateTime,
                UpdateBy = p.UpdateBy
            }).ToList();
        }

        protected string GetTagName(int tagId)
        {
            var tag = _tagRepository.GetOne(tagId);
            return tag?.TagName ?? "";
        }
    }
}
