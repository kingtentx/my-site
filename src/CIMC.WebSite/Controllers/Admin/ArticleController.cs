using System;
using System.Linq;
using CIMC.Core.Enums;
using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySite.Web.Models;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class ArticleController : ContentControllerBase
    {
        private readonly IRepository<Article> _repository;
        private readonly IPermissionService _permission;

        public ArticleController(IRepository<Article> repository, IRepository<Tag> tags, IPermissionService permission) : base(tags)
        { _repository = repository; _permission = permission; }

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
            var model = new ArticleModel { IsActive = true, Author = "中集洋山", TagsList = GetTags((int)TagType.Article) };
            if (id <= 0) return View(model);
            var entity = _repository.GetOne(id);
            if (entity == null) return NotFound();
            model = ToModel(entity); model.TagsList = GetTags((int)TagType.Article);
            return View(model);
        }

        [HttpPost, PermissionFilter(MenuCode.Content_Article, PermissionType.Edit)]
        public IActionResult Edit(int id, ArticleModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Title)) return Json(Error("请填写文章标题"));
            var entity = id > 0 ? _repository.GetOne(id) : new Article { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (entity == null) return Json(Error("记录不存在", ResultCode.NULL));
            entity.Title = input.Title; entity.Title_EN = input.Title_EN; entity.Keyword = input.Keyword;
            entity.Description = input.Description; entity.Description_EN = input.Description_EN;
            entity.Detail = input.Detail; entity.Detail_EN = input.Detail_EN;
            entity.Author = string.IsNullOrWhiteSpace(input.Author) ? "中集洋山" : input.Author;
            entity.Source = string.IsNullOrWhiteSpace(input.Source) ? "中集洋山官网" : input.Source;
            entity.SourceUrl = input.SourceUrl; entity.LinkUrl = input.LinkUrl; entity.ImageUrl = input.ImageUrl;
            entity.TagType = (int)TagType.Article; entity.TagId = input.TagId; entity.Sort = input.Sort;
            entity.IsActive = input.IsActive; entity.IsHot = input.IsHot; entity.IsDelete = false;
            entity.UpdateBy = LoginUser.UserName; entity.UpdateTime = DateTime.Now;
            if (id > 0) _repository.Update(entity); else _repository.Add(entity);
            return Json(Ok());
        }

        [HttpGet, PermissionFilter(MenuCode.Content_Article, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            var keyword = Request.Query["keywords"].ToString().Trim();
            int.TryParse(Request.Query["tagsId"], out var tagId);
            var where = LambdaHelper.True<Article>().And(p => !p.IsDelete);
            if (!string.IsNullOrWhiteSpace(keyword)) where = where.And(p => p.Title.Contains(keyword));
            if (tagId > 0) where = where.And(p => p.TagId == tagId);
            var query = _repository.GetList(where, p => p.CreationTime, Math.Max(1, pageIndex), pageSize <= 0 ? 10 : pageSize, false);
            var data = query.List.Select(p => new { p.Id, ArticleId = p.Id, p.Title, p.ImageUrl, TagName = GetTagName(p.TagId), p.CreationTime, p.ViewCount, p.ShareCount, p.IsActive, p.IsHot });
            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost, PermissionFilter(MenuCode.Content_Article, PermissionType.Edit)]
        public IActionResult SetHotArticle(int id, bool isHot)
        {
            var entity = _repository.GetOne(id); if (entity == null) return Json(Error("记录不存在", ResultCode.NULL));
            entity.IsHot = isHot; entity.UpdateTime = DateTime.Now; entity.UpdateBy = LoginUser.UserName; _repository.Update(entity);
            return Json(Ok("设置成功"));
        }

        [HttpPost, PermissionFilter(MenuCode.Content_Article, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids, int isAll = 0)
        {
            foreach (var value in (isAll == 1 ? ids ?? Array.Empty<int>() : new[] { id }).Where(p => p > 0))
            { var entity = _repository.GetOne(value); if (entity == null) continue; entity.IsDelete = true; entity.UpdateTime = DateTime.Now; entity.UpdateBy = LoginUser.UserName; _repository.Update(entity); }
            return Json(Ok("删除成功"));
        }

        private static ResultModel Ok(string message = "保存成功") => new ResultModel { Code = (int)ResultCode.Success, Message = message };
        private static ResultModel Error(string message, ResultCode code = ResultCode.ParmsError) => new ResultModel { Code = (int)code, Message = message };
        private static ArticleModel ToModel(Article p) => new ArticleModel { Id=p.Id,Title=p.Title,Title_EN=p.Title_EN,Keyword=p.Keyword,Description=p.Description,Description_EN=p.Description_EN,Detail=p.Detail,Detail_EN=p.Detail_EN,Author=p.Author,Source=p.Source,SourceUrl=p.SourceUrl,LinkUrl=p.LinkUrl,ImageUrl=p.ImageUrl,TagType=p.TagType,TagId=p.TagId,Sort=p.Sort,ViewCount=p.ViewCount,ShareCount=p.ShareCount,IsActive=p.IsActive,IsHot=p.IsHot,CreationTime=p.CreationTime,UpdateTime=p.UpdateTime,CreationBy=p.CreationBy,UpdateBy=p.UpdateBy };
    }
}
