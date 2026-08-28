using System.Globalization;
using System.Security.Cryptography;
using Sweet_Silvia_Application.Abstractions.Security;

namespace Sweet_Silvia_Infrastructure.Abstractions.Security;

internal sealed class VerificationCodeGenerator : IVerificationCodeGenerator
{
    public string Generate()
    {
        return RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6", CultureInfo.InvariantCulture);
    }
}
