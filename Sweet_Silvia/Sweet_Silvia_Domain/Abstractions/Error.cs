using System;
using System.Collections.Generic;
using System.Text;

namespace Sweet_Silvia_Domain.Abstractions;
public record Error
(
    string Code,
    string Message
)
{
    public int? StatusCode { get; init; }

    public static Error None => new(string.Empty, string.Empty);

    public static Error NullValue => new("Error.NullValue", "The value cannot be null");
}
