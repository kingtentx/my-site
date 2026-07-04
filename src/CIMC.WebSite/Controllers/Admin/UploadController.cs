using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using MySite.Web.Config;
using MySite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly UploadConfig _uploadConfig;
        private readonly IRepository<Images> _imageRepository;
        private readonly ILogger<UploadController> _logger;

        public UploadController(
            IWebHostEnvironment environment,
            IOptions<UploadConfig> uploadConfig,
            IRepository<Images> imageRepository,
            ILogger<UploadController> logger)
        {
            _environment = environment;
            _uploadConfig = uploadConfig?.Value ?? new UploadConfig();
            _imageRepository = imageRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            file ??= Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请选择文件" });
            }

            var config = _uploadConfig.Image;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = config.GetAllowedExtensions();
            if (!allowed.Contains(ext))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = $"只允许上传图片文件（{config.ExtName}）" });
            }

            var maxSize = config.GetMaxSizeBytes();
            if (file.Length > maxSize)
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = $"文件大小超过限制（最大{config.Size}MB）" });
            }

            var folder = Path.Combine(_environment.WebRootPath, config.Path, DateTime.Now.ToString("yyyyMM"));
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(folder, fileName);
            await using (var stream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/{config.Path}/{DateTime.Now:yyyyMM}/{fileName}";
            var image = new Images
            {
                FileName = Path.GetFileNameWithoutExtension(file.FileName),
                Url = url,
                ExtensionName = ext,
                Size = file.Length,
                CreationBy = User?.Identity?.Name ?? "system",
                CreationTime = DateTime.Now
            };
            _imageRepository.Add(image);

            return Json(new
            {
                code = 200,
                errno = 0,
                message = "上传成功",
                url,
                imageName = image.FileName,
                data = new[] { url }
            });
        }

        [HttpPost]
        public async Task<IActionResult> SSIUploadImage()
        {
            var urls = new List<string>();
            foreach (var file in Request.Form.Files)
            {
                var result = await UploadImage(file) as JsonResult;
                if (result?.Value != null)
                {
                    var urlProp = result.Value.GetType().GetProperty("url");
                    if (urlProp != null)
                    {
                        var url = urlProp.GetValue(result.Value)?.ToString();
                        if (!string.IsNullOrWhiteSpace(url)) urls.Add(url);
                    }
                }
            }
            return Json(new { code = 200, message = "上传成功", data = urls, urls });
        }

        [HttpGet]
        public IActionResult GetImageList(int pageIndex = 1, int pageSize = 14, string keywords = "")
        {
            var where = LambdaHelper.True<Images>().And(p => !string.IsNullOrWhiteSpace(p.Url));
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                where = where.And(p => p.FileName.Contains(keywords) || p.Url.Contains(keywords));
            }

            var query = _imageRepository.GetList(where, p => p.Id, pageIndex, pageSize, false);
            var data = query.List.Select(p => new
            {
                p.Id,
                p.FileName,
                p.Url,
                p.ExtensionName,
                p.Size,
                p.CreationBy,
                CreationTime = p.CreationTime?.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Count = query.Count, Data = data });
        }
    }
}
