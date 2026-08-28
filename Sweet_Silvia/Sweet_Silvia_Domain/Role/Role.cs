using Sweet_Silvia_Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sweet_Silvia_Domain.Role;

public sealed class Role : Entity
{
    private Role(Guid id, string name, bool isActive)
        : base(id)
    {
        Name = name;
        IsActive = isActive;
    }

    private Role()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive
    {
        get; private set;
    }

    public static Role Create(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador del rol es obligatorio.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del rol es obligatorio.", nameof(name));
        }

        return new Role(id, name.Trim(), true);
    }
}
