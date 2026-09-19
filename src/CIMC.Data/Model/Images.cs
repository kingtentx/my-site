using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>记录上传素材图片的元数据。</summary>
    public class Images : ExtCreateModel, ICreateByModel
    {
        /// <summary>主键。</summary>
        [Key]
        public long Id { get; set; }

        /// <summary>素材图片的名称。</summary>
        [StringLength(ModelUnits.Len_250)]
        public string FileName { get; set; }

        /// <summary>访问地址。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string Url { get; set; }

        /// <summary>素材图片的名称。</summary>
        [StringLength(ModelUnits.Len_10)]
        public string ExtensionName { get; set; }

        /// <summary>文件大小。</summary>
        public long Size { get; set; }

        /// <summary>创建人。</summary>
        [StringLength(ModelUnits.Len_100)]
        public string CreationBy { get; set; }
    }
}
