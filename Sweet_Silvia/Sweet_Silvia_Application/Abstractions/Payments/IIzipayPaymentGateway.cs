using Sweet_Silvia_Application.Izipay;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Abstractions.Payments;

public interface IIzipayPaymentGateway
{
    Task<Result<IzipayPaymentFormGatewayResponse>> CreatePaymentFormAsync(
        IzipayPaymentFormGatewayRequest request,
        CancellationToken cancellationToken = default);
}
