using TheraHub.Domain.Common;

namespace TheraHub.Domain.Entities;

public class Psychologist : BaseAuditableEntity
{
    public int Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string LicenseNumber { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Bio { get; private set; }
    public int UserId { get; private set; }
    public int PracticeId { get; private set; }

    public Psychologist(string firstName, string lastName, string licenseNumber, int userId, int practiceId)
    {
        PublicId = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        LicenseNumber = licenseNumber;
        UserId = userId;
        PracticeId = practiceId;
    }
}
