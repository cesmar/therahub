using TheraHub.Domain.Common;

namespace TheraHub.Domain.Entities;

public class Practice : BaseAuditableEntity
{
    public int Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }

    public Practice(string name)
    {
        PublicId = Guid.NewGuid();
        Name = name;
        IsActive = false;
    }
}
