namespace MySite.Web.Config
{
    public class UploadSetting
    {
        public string Path { get; set; } = "uploads";
        public string ExtName { get; set; } = ".gif|.png|.jpg|.jpeg|.bmp|.webp";
        public long Size { get; set; } = 10;

        public string[] GetAllowedExtensions()
        {
            if (string.IsNullOrWhiteSpace(ExtName)) return new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            return ExtName.Split('|', System.StringSplitOptions.RemoveEmptyEntries);
        }

        public long GetMaxSizeBytes() => Size * 1024 * 1024;
    }

    public class UploadConfig
    {
        public UploadSetting Image { get; set; } = new UploadSetting
        {
            Path = "uploads",
            ExtName = ".gif|.png|.jpg|.jpeg|.bmp|.webp",
            Size = 10
        };

        public UploadSetting File { get; set; } = new UploadSetting
        {
            Path = "upload/file",
            ExtName = ".doc|.docx|.xls|.xlsx|.rar|.zip",
            Size = 50
        };
    }
}
