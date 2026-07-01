namespace CIMC.Data.Entities;

public class Menu : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public int? ParentId { get; set; }

    public int Sort { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsSystem { get; set; }
}
