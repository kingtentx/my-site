namespace MySite.Web.Config
{
    /// <summary>保存文件上传的类型与大小限制。</summary>
    public class UploadSetting
    {
        /// <summary>访问路径。</summary>
        public string Path { get; set; } = "uploads";
        /// <summary>文件上传的名称。</summary>
        public string ExtName { get; set; } = ".gif|.png|.jpg|.jpeg|.bmp|.webp";
        /// <summary>文件大小。</summary>
        public long Size { get; set; } = 10;

        /// <summary>获取允许上传的文件扩展名。</summary>
        public string[] GetAllowedExtensions()
        {
            if (string.IsNullOrWhiteSpace(ExtName)) return new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            return ExtName.Split('|', System.StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>将上传大小限制换算为字节。</summary>
        public long GetMaxSizeBytes() => Size * 1024 * 1024;
    }

    /// <summary>保存文件上传的配置。</summary>
    public class UploadConfig
    {
        /// <summary>图片上传配置。</summary>
        public UploadSetting Image { get; set; } = new UploadSetting
        {
            Path = "uploads",
            ExtName = ".gif|.png|.jpg|.jpeg|.bmp|.webp",
            Size = 10
        };

        /// <summary>普通文件上传配置。</summary>
        public UploadSetting File { get; set; } = new UploadSetting
        {
            Path = "upload/file",
            ExtName = ".doc|.docx|.xls|.xlsx|.rar|.zip",
            Size = 50
        };
    }
}
