namespace CIMC.Data.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
}
