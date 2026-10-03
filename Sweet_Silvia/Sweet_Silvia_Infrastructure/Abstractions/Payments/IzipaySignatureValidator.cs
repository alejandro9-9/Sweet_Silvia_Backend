using System.Security.Cryptography;
using System.Text;
using Sweet_Silvia_Application.Abstractions.Payments;

namespace Sweet_Silvia_Infrastructure.Abstractions.Payments;

internal sealed class IzipaySignatureValidator(IzipayOptions options) : IIzipaySignatureValidator
{
    public bool IsClientConfirmationValid(string answer, string signature)
        => IsValid(answer, signature, options.HmacSha256Key);

    public bool IsNotificationValid(string answer, string signature)
        => IsValid(answer, signature, options.HmacSha256Key);

    private static bool IsValid(string answer, string signature, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(answer) ||
            string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expectedSignature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(answer)));
        byte[] providedSignature;
        try
        {
            providedSignature = Convert.FromHexString(signature.Trim());
        }
        catch (FormatException)
        {
            return false;
        }

        return providedSignature.Length == 32 &&
               CryptographicOperations.FixedTimeEquals(Convert.FromHexString(expectedSignature), providedSignature);
    }
}
