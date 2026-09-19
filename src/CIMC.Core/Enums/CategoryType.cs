using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMC.Core.Enums
{
    /// <summary>定义分类的可选类型。</summary>
    public enum CategoryType
    {
        /// <summary>
        /// 图文列表
        /// </summary>
        [Description("图文列表")]
        ImageTextList = 1,
        /// <summary>
        /// 图片列表
        /// </summary>
        [Description("图片列表")]
        ImageList = 2,
       
    }
}
