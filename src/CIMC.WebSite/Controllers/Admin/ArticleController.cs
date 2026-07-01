using CIMC.Core.Enums;
using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    [Authorize]
    public class ArticleController : ContentControllerBase
    {
        private readonly IRepository<Article> _articleRepository;
        private readonly IPermissionService _permission;

        public ArticleController(IRepository<Article> articleRepository, IRepository<Tag> tagRepository, IPermissionService permission)
            : base(tagRepository)
        {
            _articleRepository = articleRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Article, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Article, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Article, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Article, PermissionType.Delete);
            return View(GetTags((int)TagType.Article));
        }

        [PermissionFilter(MenuCode.Content_Article, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new ArticleModel { IsActive = true, Author = "中集洋山", TagId = 0, TagsList = GetTags((int)TagType.Article) };
            if (id > 0)
            {
                var article = _articleRepository.GetOne(id);
                if (article == null)
                {
                    return NotFound();
                }

                model = ToModel(article);
                model.TagsList = GetTags((int)TagType.Article);
            }

            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Article, PermissionType.Edit)]
        public IActionResult Edit(int id, ArticleModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写文章标题" };
            if (input == null || string.IsNullOrWhiteSpace(input.Title))
            {
                return Json(result);
            }

            var article = id > 0 ? _articleRepository.GetOne(id) : new Article { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (article == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            article.Title = input.Title;
            article.Title_EN = input.Title_EN;
            article.Keyword = input.Keyword;
            article.Description = input.Description;
            article.Description_EN = input.Description_EN;
            article.Detail = input.Detail;
            article.Detail_EN = input.Detail_EN;
            article.Author = string.IsNullOrWhiteSpace(input.Author) ? "中集洋山" : input.Author;
            article.Source = string.IsNullOrWhiteSpace(input.Source) ? "中集洋山官网" : input.Source;
            article.SourceUrl = input.SourceUrl;
            article.LinkUrl = input.LinkUrl;
            article.ImageUrl = input.ImageUrl;
            article.TagType = (int)TagType.Article;
            article.TagId = input.TagId;
            article.Sort = input.Sort;
            article.IsActive = input.IsActive;
            article.IsHot = input.IsHot;
            article.IsDelete = false;
            article.UpdateBy = LoginUser.UserName;
            article.UpdateTime = DateTime.Now;

            if (id > 0)
            {
                _articleRepository.Update(article);
            }
            else
            {
                _articleRepository.Add(article);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Article, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keywords = HttpContext.Request.Query["keywords"].ToString().Trim();
            _ = int.TryParse(HttpContext.Request.Query["tagsId"].ToString(), out var tagId);
            var where = LambdaHelper.True<Article>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.Title.Contains(keywords));
            }

            if (tagId > 0)
            {
                where = where.And(p => p.TagId == tagId);
            }

            var query = _articleRepository.GetList(where, p => p.CreationTime, pageIndex, pageSize, false);
            var data = query.List.Select(p => new
            {
                p.Id,
                ArticleId = p.Id,
                p.Title,
                p.ImageUrl,
                TagName = GetTagName(p.TagId),
                p.CreationTime,
                p.ViewCount,
                p.ShareCount,
                p.IsActive,
                p.IsHot
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Article, PermissionType.Edit)]
        public IActionResult SetHotArticle(int id, bool isHot)
        {
            var article = _articleRepository.GetOne(id);
            if (article == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            article.IsHot = isHot;
            article.UpdateTime = DateTime.Now;
            article.UpdateBy = LoginUser.UserName;
            _articleRepository.Update(article);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "设置成功" });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Article, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids, int isAll = 0)
        {
            var deleteIds = isAll == 1 ? ids : new[] { id };
            foreach (var deleteId in deleteIds.Where(p => p > 0))
            {
                var article = _articleRepository.GetOne(deleteId);
                if (article != null)
                {
                    article.IsDelete = true;
                    article.UpdateTime = DateTime.Now;
                    article.UpdateBy = LoginUser.UserName;
                    _articleRepository.Update(article);
                }
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private ArticleModel ToModel(Article article)
        {
            return new ArticleModel
            {
                Id = article.Id,
                Title = article.Title,
                Title_EN = article.Title_EN,
                Keyword = article.Keyword,
                Description = article.Description,
                Description_EN = article.Description_EN,
                Detail = article.Detail,
                Detail_EN = article.Detail_EN,
                Author = article.Author,
                Source = article.Source,
                SourceUrl = article.SourceUrl,
                LinkUrl = article.LinkUrl,
                ImageUrl = article.ImageUrl,
                TagType = article.TagType,
                TagId = article.TagId,
                Sort = article.Sort,
                ViewCount = article.ViewCount,
                ShareCount = article.ShareCount,
                IsActive = article.IsActive,
                IsHot = article.IsHot,
                CreationTime = article.CreationTime,
                UpdateTime = article.UpdateTime,
                CreationBy = article.CreationBy,
                UpdateBy = article.UpdateBy
            };
        }
    }
}
