using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.Address;

public sealed class Address : Entity
{
    private Address() { }

    public Address(Guid id, Guid userId, string receiverName, PhoneNumber receiverPhone, string country, Guid departmentId, Guid provinceId, Guid districtId, AddressLine line, string? references, bool isDefault, bool isActive, DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        ReceiverName = receiverName;
        ReceiverPhone = receiverPhone;
        Country = country;
        DepartmentId = departmentId;
        ProvinceId = provinceId;
        DistrictId = districtId;
        Line = line;
        References = references;
        IsDefault = isDefault;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }
    public string ReceiverName { get; private set; } = string.Empty;
    public PhoneNumber ReceiverPhone { get; private set; } = null!;
    public string Country { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public Guid ProvinceId { get; private set; }
    public Guid DistrictId { get; private set; }
    public AddressLine Line { get; private set; } = null!;
    public string? References { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Address Create(Guid userId, string receiverName, PhoneNumber receiverPhone, Guid departmentId, Guid provinceId, Guid districtId, AddressLine line, string? references, bool isDefault)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        Validate(receiverName, receiverPhone, departmentId, provinceId, districtId, line);

        var result = new Address(Guid.NewGuid(), userId, receiverName.Trim(), receiverPhone, "PE", departmentId, provinceId, districtId, line, references?.Trim(), isDefault, true, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new AddressCreatedEvent(result.Id));
        return result;
    }

    public void Update(
        string receiverName,
        PhoneNumber receiverPhone,
        Guid departmentId,
        Guid provinceId,
        Guid districtId,
        AddressLine line,
        string? references,
        bool isDefault)
    {
        Validate(receiverName, receiverPhone, departmentId, provinceId, districtId, line);

        ReceiverName = receiverName.Trim();
        ReceiverPhone = receiverPhone;
        DepartmentId = departmentId;
        ProvinceId = provinceId;
        DistrictId = districtId;
        Line = line;
        References = references?.Trim();
        IsDefault = isDefault;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AddressUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AddressDeletedEvent(Id));
    }

    private static void Validate(string receiverName, PhoneNumber receiverPhone, Guid departmentId, Guid provinceId, Guid districtId, AddressLine line)
    {
        if (string.IsNullOrWhiteSpace(receiverName))
        {
            throw new ArgumentException("Receiver name is required.", nameof(receiverName));
        }

        if (receiverPhone is null)
        {
            throw new ArgumentNullException(nameof(receiverPhone));
        }

        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("Department id is required.", nameof(departmentId));
        }

        if (provinceId == Guid.Empty)
        {
            throw new ArgumentException("Province id is required.", nameof(provinceId));
        }

        if (districtId == Guid.Empty)
        {
            throw new ArgumentException("District id is required.", nameof(districtId));
        }

        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }
    }
}
