using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 附件
    /// </summary>
    public class Attachments : ExtCreateModel, ICreateByModel
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 图片名称
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string FileName { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Url { get; set; }
        /// <summary>
        /// 扩展名
        /// </summary>
        [StringLength(ModelUnits.Len_10)]
        public string ExtensionName { get; set; }
        /// <summary>
        /// MD5值
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string MD5 { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string CreationBy { get; set; }
    }
}
