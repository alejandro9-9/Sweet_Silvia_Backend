using Sweet_Silvia_Domain.PaymentMethod;

namespace Sweet_Silvia_Tests;

public sealed class PaymentMethodTests
{
    [Fact]
    public void Activate_restores_a_deactivated_payment_method()
    {
        var method = PaymentMethod.Create("Yape", PaymentMethodType.Manual, true, false);

        method.Delete();
        method.Activate();

        Assert.True(method.IsActive);
    }

    [Fact]
    public void Activate_is_idempotent_for_an_active_payment_method()
    {
        var method = PaymentMethod.Create("Yape", PaymentMethodType.Manual, true, false);

        method.Activate();

        Assert.True(method.IsActive);
    }
}
