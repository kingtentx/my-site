using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// ����
    /// </summary>
    public class Article : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_250)]
        public string Title { get; set; }
        /// <summary>
        /// 标题（英文）
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Title_EN { get; set; }
        /// <summary>
        /// 关键词
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Keyword { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Description { get; set; }
        /// <summary>
        /// 描述（英文）
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Description_EN { get; set; }
        /// <summary>
        /// 详情
        /// </summary>      
        public string Detail { get; set; }
        /// <summary>
        /// 详情（英文）
        /// </summary>
        public string Detail_EN { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string Author { get; set; }
        /// <summary>
        /// ��Դ
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string Source { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string SourceUrl { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string LinkUrl { get; set; }
        /// <summary>
        /// ͼƬ
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string ImageUrl { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public int TagType { get; set; }
        /// <summary>
        /// ��ǩ
        /// </summary>
        public int TagId { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// �����
        /// </summary>
        public int ViewCount { get; set; } = 0;
        /// <summary>
        /// ������
        /// </summary>
        public int ShareCount { get; set; } = 0;
        /// <summary>
        /// �Ƿ�����
        /// </summary>
        public bool IsHot { get; set; }
        /// <summary>
        /// �Ƿ񷢱�
        /// </summary>
        public bool IsActive { get; set; }

        public bool IsDelete { get; set; } = false;


    }
}
