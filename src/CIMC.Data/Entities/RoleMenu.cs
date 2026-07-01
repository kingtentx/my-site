namespace CIMC.Data.Entities;

public class RoleMenu : BaseEntity
{
    public int RoleId { get; set; }

    public Role? Role { get; set; }

    public int MenuId { get; set; }

    public Menu? Menu { get; set; }

    public bool CanView { get; set; }

    public bool CanAdd { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }
}
