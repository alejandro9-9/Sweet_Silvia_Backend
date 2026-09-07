namespace Sweet_Silvia_Application.Abstractions.Payments;

public interface IIzipaySignatureValidator
{
    bool IsValid(string payload, string signature);
}
