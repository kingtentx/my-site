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
    public class AlbumController : ContentControllerBase
    {
        private readonly IRepository<Album> _albumRepository;
        private readonly IPermissionService _permission;

        public AlbumController(IRepository<Album> albumRepository, IRepository<Tag> tagRepository, IPermissionService permission)
            : base(tagRepository)
        {
            _albumRepository = albumRepository;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.Content_Album, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Add] = _permission.CheckPermission(LoginUser, MenuCode.Content_Album, PermissionType.Add);
            ViewData[PageCode.PAGE_Button_Edit] = _permission.CheckPermission(LoginUser, MenuCode.Content_Album, PermissionType.Edit);
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.Content_Album, PermissionType.Delete);
            return View(GetTags((int)TagType.Image));
        }

        [PermissionFilter(MenuCode.Content_Album, PermissionType.Edit)]
        public IActionResult Edit(int id = 0)
        {
            var model = new AlbumModel { IsActive = true, Author = "中集洋山", TagId = 0, TagsList = GetTags((int)TagType.Image) };
            if (id > 0)
            {
                var album = _albumRepository.GetOne(id);
                if (album == null)
                {
                    return NotFound();
                }

                model = ToModel(album);
                model.TagsList = GetTags((int)TagType.Image);
            }

            return View(model);
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Album, PermissionType.Edit)]
        public IActionResult Edit(int id, AlbumModel input)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请填写标题并上传图片" };
            if (input == null || string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.ImageUrl))
            {
                return Json(result);
            }

            var album = id > 0 ? _albumRepository.GetOne(id) : new Album { CreationTime = DateTime.Now, CreationBy = LoginUser.UserName };
            if (album == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            album.Title = input.Title;
            album.Title_EN = input.Title_EN;
            album.Description = input.Description;
            album.Description_EN = input.Description_EN;
            album.Detail = input.Detail;
            album.Detail_EN = input.Detail_EN;
            album.ImageUrl = input.ImageUrl;
            album.LinkUrl = input.LinkUrl;
            album.Author = string.IsNullOrWhiteSpace(input.Author) ? "中集洋山" : input.Author;
            album.TagType = (int)TagType.Image;
            album.TagId = input.TagId;
            album.Sort = input.Sort;
            album.IsActive = input.IsActive;
            album.IsDelete = false;
            album.UpdateBy = LoginUser.UserName;
            album.UpdateTime = DateTime.Now;

            if (id > 0)
            {
                _albumRepository.Update(album);
            }
            else
            {
                _albumRepository.Add(album);
            }

            result.Code = (int)ResultCode.Success;
            result.Message = "保存成功";
            return Json(result);
        }

        [PermissionFilter(MenuCode.Content_Album, PermissionType.Add)]
        public IActionResult BatchUpload()
        {
            return View(new AlbumModel { IsActive = true, Author = "中集洋山", TagId = 0, TagsList = GetTags((int)TagType.Image) });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Album, PermissionType.Add)]
        public IActionResult BatchUpload(AlbumModel input)
        {
            var images = input.ImageList ?? new List<string>();
            foreach (var image in images.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                _albumRepository.Add(new Album
                {
                    Title = string.IsNullOrWhiteSpace(input.Title) ? "图片" : input.Title,
                    Title_EN = input.Title_EN,
                    Description = input.Description,
                    Description_EN = input.Description_EN,
                    Detail = input.Detail,
                    Detail_EN = input.Detail_EN,
                    ImageUrl = image,
                    LinkUrl = input.LinkUrl,
                    Author = string.IsNullOrWhiteSpace(input.Author) ? "中集洋山" : input.Author,
                    TagType = (int)TagType.Image,
                    TagId = input.TagId,
                    Sort = input.Sort,
                    IsActive = input.IsActive,
                    IsDelete = false,
                    CreationBy = LoginUser.UserName,
                    CreationTime = DateTime.Now
                });
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "保存成功" });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.Content_Album, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 10)
        {
            _ = int.TryParse(HttpContext.Request.Query["tagsId"].ToString(), out var tagId);
            var where = LambdaHelper.True<Album>().And(p => !p.IsDelete);
            if (tagId > 0)
            {
                where = where.And(p => p.TagId == tagId);
            }

            var query = _albumRepository.GetList(where, p => p.Sort, pageIndex, pageSize, true);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.ImageUrl,
                p.LinkUrl,
                p.Title,
                p.Description,
                p.Sort,
                TagName = GetTagName(p.TagId),
                p.CreationTime,
                p.IsActive
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Message = "成功", Count = query.Count, Data = data });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.Content_Album, PermissionType.Delete)]
        public IActionResult Delete(int id, int[] ids, int isAll = 0)
        {
            var deleteIds = isAll == 1 ? ids : new[] { id };
            foreach (var deleteId in deleteIds.Where(p => p > 0))
            {
                var album = _albumRepository.GetOne(deleteId);
                if (album != null)
                {
                    album.IsDelete = true;
                    album.UpdateTime = DateTime.Now;
                    album.UpdateBy = LoginUser.UserName;
                    _albumRepository.Update(album);
                }
            }

            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = "删除成功" });
        }

        private AlbumModel ToModel(Album album)
        {
            return new AlbumModel
            {
                Id = album.Id,
                Title = album.Title,
                Title_EN = album.Title_EN,
                Description = album.Description,
                Description_EN = album.Description_EN,
                Detail = album.Detail,
                Detail_EN = album.Detail_EN,
                ImageUrl = album.ImageUrl,
                LinkUrl = album.LinkUrl,
                Author = album.Author,
                TagType = album.TagType,
                TagId = album.TagId,
                Sort = album.Sort,
                IsActive = album.IsActive,
                IsDelete = album.IsDelete,
                CreationTime = album.CreationTime,
                UpdateTime = album.UpdateTime,
                CreationBy = album.CreationBy,
                UpdateBy = album.UpdateBy
            };
        }
    }
}
