namespace CIMC.WebSite.Config;

public class AuditLogOptions
{
    public bool IsEnabled { get; set; } = true;

    public string[] RecordOperations { get; set; } = Array.Empty<string>();
}
