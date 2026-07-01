namespace CIMC.Data.Entities;

public class AuditLog : BaseEntity
{
    public string? UserName { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? Ip { get; set; }

    public string? UserAgent { get; set; }

    public int StatusCode { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string? RequestBody { get; set; }
}
