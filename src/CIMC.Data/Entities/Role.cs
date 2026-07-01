namespace CIMC.Data.Entities;

public class Role : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public List<RoleMenu> RoleMenus { get; set; } = new();
}
