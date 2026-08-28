using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Province;

public sealed class Province : Entity
{
    private Province() { }

    public Province(Guid id, Guid departmentId, string name, bool isActive)
        : base(id)
    {
        DepartmentId = departmentId;
        Name = name;
        IsActive = isActive;
    }

    public Guid DepartmentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public static Province Create(Guid departmentId, string name)
    {
        Validate(departmentId, name);

        return new Province(Guid.NewGuid(), departmentId, name.Trim(), true);
    }

    public void Update(Guid departmentId, string name)
    {
        Validate(departmentId, name);

        DepartmentId = departmentId;
        Name = name.Trim();
    }

    public void Delete()
    {
        IsActive = false;
    }

    private static void Validate(Guid departmentId, string name)
    {
        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("Department id is required.", nameof(departmentId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Province name is required.", nameof(name));
        }
    }
}
