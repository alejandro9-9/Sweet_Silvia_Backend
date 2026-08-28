using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.District;

public sealed class District : Entity
{
    private District() { }

    public District(Guid id, Guid provinceId, string name, string? ubigeo, bool isActive)
        : base(id)
    {
        ProvinceId = provinceId;
        Name = name;
        Ubigeo = ubigeo;
        IsActive = isActive;
    }

    public Guid ProvinceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Ubigeo { get; private set; }
    public bool IsActive { get; private set; }

    public static District Create(Guid provinceId, string name, string? ubigeo = null)
    {
        Validate(provinceId, name);

        return new District(Guid.NewGuid(), provinceId, name.Trim(), ubigeo?.Trim(), true);
    }

    public void Update(Guid provinceId, string name, string? ubigeo = null)
    {
        Validate(provinceId, name);

        ProvinceId = provinceId;
        Name = name.Trim();
        Ubigeo = ubigeo?.Trim();
    }

    public void Delete()
    {
        IsActive = false;
    }

    private static void Validate(Guid provinceId, string name)
    {
        if (provinceId == Guid.Empty)
        {
            throw new ArgumentException("Province id is required.", nameof(provinceId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("District name is required.", nameof(name));
        }
    }
}
