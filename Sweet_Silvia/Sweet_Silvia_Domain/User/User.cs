using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.User;

public sealed class User : Entity
{
    private User() { }

    public User(Guid id, Guid roleId, string name, string paternalSurname, string? maternalSurname, PhoneNumber? phone, EmailAddress email, string? passwordHash, UserStatus status, DateTimeOffset createdAt, string? googleSubject = null)
        : base(id)
    {
        RoleId = roleId;
        Name = name;
        PaternalSurname = paternalSurname;
        MaternalSurname = maternalSurname;
        Phone = phone;
        Email = email;
        PasswordHash = passwordHash;
        Status = status;
        CreatedAt = createdAt;
        GoogleSubject = googleSubject;
    }

    public Guid RoleId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string PaternalSurname { get; private set; } = string.Empty;
    public string? MaternalSurname { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public EmailAddress Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string? GoogleSubject { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public bool IsActive => Status == UserStatus.Active;

    public static User Create(Guid roleId, string name, string paternalSurname, string? maternalSurname, PhoneNumber phone, EmailAddress email, string passwordHash)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(paternalSurname))
        {
            throw new ArgumentException("Paternal surname is required.", nameof(paternalSurname));
        }

        if (phone is null)
        {
            throw new ArgumentNullException(nameof(phone));
        }

        if (email is null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        var result = new User(Guid.NewGuid(), roleId, name.Trim(), paternalSurname.Trim(), maternalSurname?.Trim(), phone, email, passwordHash, UserStatus.Active, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new UserCreatedEvent(result.Id));
        return result;
    }

    public static User CreateFromGoogle(Guid roleId, string name, string paternalSurname, string? maternalSurname, PhoneNumber? phone, EmailAddress email, string googleSubject)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(paternalSurname))
        {
            throw new ArgumentException("Paternal surname is required.", nameof(paternalSurname));
        }

        if (email is null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        if (string.IsNullOrWhiteSpace(googleSubject))
        {
            throw new ArgumentException("Google subject is required.", nameof(googleSubject));
        }

        var result = new User(
            Guid.NewGuid(),
            roleId,
            name.Trim(),
            paternalSurname.Trim(),
            string.IsNullOrWhiteSpace(maternalSurname) ? null : maternalSurname.Trim(),
            phone,
            email,
            null,
            UserStatus.Active,
            DateTimeOffset.UtcNow,
            googleSubject.Trim());

        result.AddDomainEvent(new UserCreatedEvent(result.Id));
        return result;
    }

    public void LinkGoogleAccount(string googleSubject)
    {
        if (string.IsNullOrWhiteSpace(googleSubject))
        {
            throw new ArgumentException("Google subject is required.", nameof(googleSubject));
        }

        if (GoogleSubject is not null && !string.Equals(GoogleSubject, googleSubject, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The user is already linked to another Google account.");
        }

        GoogleSubject = googleSubject.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserUpdatedEvent(Id));
    }

    public void ChangeRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(roleId));
        }

        RoleId = roleId;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserUpdatedEvent(Id));
    }

    public void UpdateProfile(string name, string paternalSurname, string? maternalSurname, PhoneNumber phone, EmailAddress email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(paternalSurname))
        {
            throw new ArgumentException("Paternal surname is required.", nameof(paternalSurname));
        }

        if (phone is null)
        {
            throw new ArgumentNullException(nameof(phone));
        }

        if (email is null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        Name = name.Trim();
        PaternalSurname = paternalSurname.Trim();
        MaternalSurname = string.IsNullOrWhiteSpace(maternalSurname) ? null : maternalSurname.Trim();
        Phone = phone;
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserUpdatedEvent(Id));
    }

    public void Block()
    {
        Status = UserStatus.Blocked;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserBlockedEvent(Id));
    }

    public void Unblock()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserUpdatedEvent(Id));
    }
}
