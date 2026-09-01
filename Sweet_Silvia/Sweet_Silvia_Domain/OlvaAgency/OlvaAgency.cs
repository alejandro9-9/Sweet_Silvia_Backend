using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.OlvaAgency;

public sealed class OlvaAgency : Entity
{
    private OlvaAgency() { }

    public OlvaAgency(
        Guid id,
        string code,
        string name,
        string? type,
        string? department,
        string? province,
        string? district,
        string? address,
        string? phone,
        string? scheduleJson,
        double? latitude,
        double? longitude,
        bool isActive,
        DateTimeOffset lastSyncedAt)
        : base(id)
    {
        Code = code;
        Name = name;
        Type = type;
        Department = department;
        Province = province;
        District = district;
        Address = address;
        Phone = phone;
        ScheduleJson = scheduleJson;
        Latitude = latitude;
        Longitude = longitude;
        IsActive = isActive;
        LastSyncedAt = lastSyncedAt;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Type { get; private set; }
    public string? Department { get; private set; }
    public string? Province { get; private set; }
    public string? District { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? ScheduleJson { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset LastSyncedAt { get; private set; }

    public static OlvaAgency Create(
        string code,
        string name,
        string? type,
        string? department,
        string? province,
        string? district,
        string? address,
        string? phone,
        string? scheduleJson,
        double? latitude,
        double? longitude)
    {
        Validate(code, name);

        return new OlvaAgency(
            Guid.NewGuid(),
            code.Trim(),
            name.Trim(),
            Trim(type),
            Trim(department),
            Trim(province),
            Trim(district),
            Trim(address),
            Trim(phone),
            Trim(scheduleJson),
            latitude,
            longitude,
            true,
            DateTimeOffset.UtcNow);
    }

    public void UpdateFromOlva(
        string name,
        string? type,
        string? department,
        string? province,
        string? district,
        string? address,
        string? phone,
        string? scheduleJson,
        double? latitude,
        double? longitude)
    {
        Validate(Code, name);

        Name = name.Trim();
        Type = Trim(type);
        Department = Trim(department);
        Province = Trim(province);
        District = Trim(district);
        Address = Trim(address);
        Phone = Trim(phone);
        ScheduleJson = Trim(scheduleJson);
        Latitude = latitude;
        Longitude = longitude;
        IsActive = true;
        LastSyncedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastSyncedAt = DateTimeOffset.UtcNow;
    }

    private static void Validate(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Olva agency code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Olva agency name is required.", nameof(name));
        }
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
