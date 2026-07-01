using System.ComponentModel;

namespace CIMC.Core.Enums
{
    public enum TagType
    {
        [Description("文章")]
        Article = 1,
        [Description("图片")]
        Image = 2,
        [Description("招聘")]
        Job = 3,
    }
}
