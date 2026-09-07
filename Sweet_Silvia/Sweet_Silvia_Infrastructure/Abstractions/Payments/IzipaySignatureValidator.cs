using System.Security.Cryptography;
using System.Text;
using Sweet_Silvia_Application.Abstractions.Payments;

namespace Sweet_Silvia_Infrastructure.Abstractions.Payments;

internal sealed class IzipaySignatureValidator(IzipayOptions options) : IIzipaySignatureValidator
{
    public bool IsValid(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(options.HashKey) ||
            string.IsNullOrWhiteSpace(payload) ||
            string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.HashKey));
        var expectedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signature.Trim()));
    }
}
