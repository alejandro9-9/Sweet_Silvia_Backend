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

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del rol es obligatorio.", nameof(name));
        }

        var normalizedName = name.Trim();
        if (IsSystemRole && !string.Equals(Name, normalizedName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Los roles del sistema no se pueden renombrar.");
        }

        Name = normalizedName;
    }

    public void Delete()
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("Los roles del sistema no se pueden desactivar.");
        }

        IsActive = false;
    }

    private bool IsSystemRole =>
        Id == RoleIds.Customer ||
        Id == RoleIds.Administrator ||
        Id == RoleIds.Assistant;
}
