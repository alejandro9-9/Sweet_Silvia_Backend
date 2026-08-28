using Sweet_Silvia_Application.Abstractions.Time;

namespace Sweet_Silvia_Infrastructure.Abstractions.Time;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
