using TheraHub.Domain.Common;

namespace TheraHub.Domain.Entities;

public class User : BaseAuditableEntity
{
    public int Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public int PracticeId { get; private set; }

    public User(string email, string passwordHash, int practiceId)
    {
        PublicId = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        PracticeId = practiceId;
        IsActive = true;
        IsEmailConfirmed = false;
    }
}
