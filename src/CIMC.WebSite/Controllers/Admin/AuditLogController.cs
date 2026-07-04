using CIMC.Data;
using CIMC.EntityFramework;
using MySite.Web.Models;
using MySite.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MySite.Web.Controllers
{
    [Authorize]
    public class AuditLogController : AdminBaseController
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IPermissionService _permission;

        public AuditLogController(IAuditLogService auditLogService, IPermissionService permission)
        {
            _auditLogService = auditLogService;
            _permission = permission;
        }

        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.View)]
        public IActionResult Index()
        {
            ViewData[PageCode.PAGE_Button_Delete] = _permission.CheckPermission(LoginUser, MenuCode.System_AuditLog, PermissionType.Delete);
            return View();
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.View)]
        public JsonResult GetList(int pageIndex = 1, int pageSize = 15)
        {
            var userId = HttpContext.Request.Query["userId"].ToString().Trim();
            var userName = HttpContext.Request.Query["userName"].ToString().Trim();
            var operationType = HttpContext.Request.Query["operationType"].ToString().Trim();
            var operationModule = HttpContext.Request.Query["operationModule"].ToString().Trim();
            var resultStatus = HttpContext.Request.Query["resultStatus"].ToString().Trim();
            var startTime = HttpContext.Request.Query["startTime"].ToString().Trim();
            var endTime = HttpContext.Request.Query["endTime"].ToString().Trim();

            var (list, count) = _auditLogService.GetList(
                userId, userName, operationType, operationModule, resultStatus,
                startTime, endTime, pageIndex, pageSize);

            var data = list.Select(p => new
            {
                p.Id,
                p.UserId,
                p.UserName,
                OperationTime = p.OperationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                p.IpAddress,
                p.OperationType,
                p.OperationModule,
                p.OperationDesc,
                p.RequestUrl,
                p.HttpMethod,
                p.ResultStatus,
                p.ResultMessage,
                p.Duration,
                p.OperationTable,
                p.RecordId,
                p.IsArchived
            }).ToList();

            return Json(new ResultModel<object> { Code = (int)ResultCode.Success, Count = count, Data = data });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.View)]
        public JsonResult GetDetail(long id)
        {
            var repository = HttpContext.RequestServices.GetService(typeof(IRepository<AuditLog>)) as IRepository<AuditLog>;
            var log = repository?.GetOne(id);
            if (log == null)
            {
                return Json(new ResultModel { Code = (int)ResultCode.NULL, Message = "记录不存在" });
            }

            var hashValid = _auditLogService.VerifyHash(id);

            return Json(new ResultModel<object>
            {
                Code = (int)ResultCode.Success,
                Data = new
                {
                    log.Id,
                    log.UserId,
                    log.UserName,
                    OperationTime = log.OperationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    log.IpAddress,
                    log.OperationType,
                    log.OperationModule,
                    log.OperationDesc,
                    log.RequestUrl,
                    log.HttpMethod,
                    log.RequestData,
                    log.OldData,
                    log.NewData,
                    log.ResultStatus,
                    log.ResultMessage,
                    log.Duration,
                    log.DataHash,
                    log.UserAgent,
                    log.OperationTable,
                    log.RecordId,
                    log.IsArchived,
                    HashValid = hashValid
                }
            });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.View)]
        public IActionResult ExportExcel(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime)
        {
            var list = _auditLogService.Export(
                userId, userName, operationType, operationModule, resultStatus,
                startTime, endTime);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("审计日志");

            var headers = new[] { "ID", "用户ID", "用户名", "操作时间", "IP地址", "操作类型", "操作模块", "操作描述", "请求地址", "HTTP方法", "结果状态", "结果消息", "耗时(ms)", "操作表", "记录ID", "用户代理" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var row = i + 2;
                worksheet.Cells[row, 1].Value = item.Id;
                worksheet.Cells[row, 2].Value = item.UserId;
                worksheet.Cells[row, 3].Value = item.UserName;
                worksheet.Cells[row, 4].Value = item.OperationTime.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 5].Value = item.IpAddress;
                worksheet.Cells[row, 6].Value = item.OperationType;
                worksheet.Cells[row, 7].Value = item.OperationModule;
                worksheet.Cells[row, 8].Value = item.OperationDesc;
                worksheet.Cells[row, 9].Value = item.RequestUrl;
                worksheet.Cells[row, 10].Value = item.HttpMethod;
                worksheet.Cells[row, 11].Value = item.ResultStatus;
                worksheet.Cells[row, 12].Value = item.ResultMessage;
                worksheet.Cells[row, 13].Value = item.Duration;
                worksheet.Cells[row, 14].Value = item.OperationTable;
                worksheet.Cells[row, 15].Value = item.RecordId;
                worksheet.Cells[row, 16].Value = item.UserAgent;
            }

            worksheet.Cells.AutoFitColumns();

            var bytes = package.GetAsByteArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"审计日志_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.View)]
        public IActionResult ExportCsv(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime)
        {
            var list = _auditLogService.Export(
                userId, userName, operationType, operationModule, resultStatus,
                startTime, endTime);

            var sb = new StringBuilder();
            sb.AppendLine("ID,用户ID,用户名,操作时间,IP地址,操作类型,操作模块,操作描述,请求地址,HTTP方法,结果状态,结果消息,耗时(ms)");
            foreach (var item in list)
            {
                sb.AppendLine($"{item.Id},{item.UserId},{item.UserName},{item.OperationTime:yyyy-MM-dd HH:mm:ss},{item.IpAddress},{item.OperationType},{item.OperationModule},{item.OperationDesc},{item.RequestUrl},{item.HttpMethod},{item.ResultStatus},{item.ResultMessage},{item.Duration}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var bom = Encoding.UTF8.GetPreamble();
            var result = bom.Concat(bytes).ToArray();

            return File(result, "text/csv", $"审计日志_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.View)]
        public JsonResult GetStats(string startTime, string endTime)
        {
            var typeStats = _auditLogService.GetOperationTypeStats(startTime, endTime);
            var moduleStats = _auditLogService.GetModuleStats(startTime, endTime);

            return Json(new ResultModel<object>
            {
                Code = (int)ResultCode.Success,
                Data = new
                {
                    TypeStats = typeStats,
                    ModuleStats = moduleStats
                }
            });
        }

        [HttpGet]
        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.View)]
        public JsonResult VerifyIntegrity(long? id)
        {
            if (id.HasValue)
            {
                var valid = _auditLogService.VerifyHash(id.Value);
                return Json(new ResultModel<object>
                {
                    Code = (int)ResultCode.Success,
                    Data = new { Id = id.Value, IsValid = valid }
                });
            }

            var (total, tampered) = _auditLogService.VerifyAllHashes();
            return Json(new ResultModel<object>
            {
                Code = (int)ResultCode.Success,
                Data = new { Total = total, Tampered = tampered, IsValid = tampered == 0 }
            });
        }

        [HttpPost]
        [PermissionFilter(MenuCode.System_AuditLog, PermissionType.Delete)]
        public JsonResult Archive(string beforeDate)
        {
            if (!DateTime.TryParse(beforeDate, out var date))
            {
                return Json(new ResultModel { Code = (int)ResultCode.ParmsError, Message = "日期格式不正确" });
            }

            var count = _auditLogService.ArchiveLogs(date);
            return Json(new ResultModel { Code = (int)ResultCode.Success, Message = $"已归档 {count} 条日志" });
        }
    }
}
