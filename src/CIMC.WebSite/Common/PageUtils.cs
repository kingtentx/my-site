using CIMC.Data;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CimcSite.Web
{
    public class PageUtils
    {
        /// <summary>
        /// 菜单递归
        /// </summary>
        /// <param name="list"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static List<TreeSelectModel> TreeSelect(List<Menu> list, TreeSelectModel tree)
        {
            int parentId = tree.Id;//根节点ID

            var treeList = new List<TreeSelectModel>();

            var children = list.Where(t => t.Pid == parentId);
            foreach (var chl in children)
            {
                var model = new TreeSelectModel();
                model.Id = chl.Id;
                model.Name = chl.Title;
                model.Sort = chl.Sort;

                var nodes = TreeSelect(list, model);
                model.Children = nodes.Count() > 0 ? nodes : null;
                treeList.Add(model);
            }
            return treeList;
        }

        /// <summary>
        /// 文件保存到服务器
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="serverPath"></param>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public static async Task<bool> Save(Stream stream, string serverPath, string saveName)
        {
            try
            {
                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                await Task.Run(() =>
                {
                    using (FileStream fs = new FileStream(serverPath + saveName, FileMode.Create))
                    {
                        stream.Position = 0;
                        stream.CopyTo(fs);
                        fs.Close();
                    }
                });
                return true;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="tmpDirectory"></param>
        /// <param name="serverPath"></param>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public static async Task<bool> FileMerge(string tmpDirectory, string serverPath, string saveName)
        {
            try
            {
                var tmpPath = serverPath + tmpDirectory;//获得临时目录下面的所有文件

                var files = Directory.GetFiles(tmpPath);

                using (var fs = new FileStream(serverPath + saveName, FileMode.Create))
                {
                    foreach (var part in files.OrderBy(x => x.Length).ThenBy(x => x))
                    {
                        var bytes = System.IO.File.ReadAllBytes(part);
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                        bytes = null;
                        System.IO.File.Delete(part);//删除分块
                    }
                    fs.Close();

                    Directory.Delete(tmpPath);//删除临时目录
                    return true;
                }
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 获取IP
        /// </summary>
        public static string GetIPAddress()
        {
            var httpContextAccessor = new HttpContextAccessor();
            var ip = httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();//X-Forwarded-For可能会包含多个IP
            if (string.IsNullOrEmpty(ip))
            {
                return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else
            {
                return ip.IndexOf(',') > 0 ? ip.Split(',')[0] : ip;
            }
        }

    }

}
