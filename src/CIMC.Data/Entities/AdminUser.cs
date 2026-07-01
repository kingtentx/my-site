namespace CIMC.Data.Entities;

public class AdminUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int RoleId { get; set; }

    public Role? Role { get; set; }
}
