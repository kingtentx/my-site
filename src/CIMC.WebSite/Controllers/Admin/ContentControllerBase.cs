using System.Collections.Generic;
using System.Linq;
using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using MySite.Web.Models;

namespace MySite.Web.Controllers
{
    public abstract class ContentControllerBase : AdminBaseController
    {
        private readonly IRepository<Tag> _tagRepository;

        protected ContentControllerBase(IRepository<Tag> tagRepository) => _tagRepository = tagRepository;

        protected List<TagModel> GetTags(int? tagType = null)
        {
            var where = LambdaHelper.True<Tag>().And(p => p.IsActive);
            if (tagType.HasValue)
            {
                var type = tagType.Value;
                where = where.And(p => p.TagType == type);
            }
            return _tagRepository.GetList(where, p => p.Sort, 1, 500, true).List.Select(ToTagModel).ToList();
        }

        protected string GetTagName(int tagId) => _tagRepository.GetOne(tagId)?.TagName ?? string.Empty;

        protected static TagModel ToTagModel(Tag tag) => new TagModel
        {
            Id = tag.Id, TagName = tag.TagName, TagName_EN = tag.TagName_EN, TagType = tag.TagType,
            Sort = tag.Sort, IsActive = tag.IsActive, CreationTime = tag.CreationTime,
            CreationBy = tag.CreationBy, UpdateTime = tag.UpdateTime, UpdateBy = tag.UpdateBy
        };
    }
}
