using System;
using System.Collections.Generic;
using System.Text;

namespace Sweet_Silvia_Domain.Abstractions;

public abstract class Entity
{
    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity()
    {
    }

    public Guid Id
    {
        get; init;
    }

    private readonly List<IDomainEvent> _domainEvents = [];

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() =>
        _domainEvents.AsReadOnly();
}
