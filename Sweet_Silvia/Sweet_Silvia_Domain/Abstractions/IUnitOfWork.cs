using System;
using System.Collections.Generic;
using System.Text;

namespace Sweet_Silvia_Domain.Abstractions;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
