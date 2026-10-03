namespace Sweet_Silvia_Application.Abstractions.Payments;

public interface IIzipaySignatureValidator
{
	bool IsClientConfirmationValid(string answer, string signature);
	bool IsNotificationValid(string answer, string signature);
}
