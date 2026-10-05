using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Tests;

public sealed class UserFacingErrorMessagesTests
{
    [Theory]
    [InlineData("Cart.StockNotEnough", "No hay suficientes unidades disponibles. Ajusta la cantidad o elige otra variante.")]
    [InlineData("Coupon.TotalUsesExceeded", "Este cupón alcanzó el límite total de usos.")]
    [InlineData("Izipay.CheckoutDetailsRequired", "Completa los datos de contacto y el método de pago para iniciar el pago con Izipay.")]
    [InlineData("User.EmailAlreadyExists", "Ya existe una cuenta con ese correo electrónico.")]
    public void Localize_returns_actionable_message_for_business_error_codes(string code, string expectedMessage)
    {
        var error = new Error(code, "Internal English message") { StatusCode = 409 };

        var localized = UserFacingErrorMessages.Localize(error);

        Assert.Equal(expectedMessage, localized.Message);
        Assert.Equal(409, localized.StatusCode);
        Assert.Equal(code, localized.Code);
    }

    [Fact]
    public void Localize_describes_missing_resources_without_exposing_internal_ids()
    {
        var error = new Error("Product.NotFound", "Product with id 'internal-id' was not found.") { StatusCode = 404 };

        var localized = UserFacingErrorMessages.Localize(error);

        Assert.Equal("No encontramos el producto.", localized.Message);
        Assert.DoesNotContain("internal-id", localized.Message);
    }

    [Theory]
    [InlineData(401, "Tu sesión venció o ya no es válida. Inicia sesión nuevamente.")]
    [InlineData(403, "No tienes permiso para realizar esta acción.")]
    [InlineData(413, "El archivo supera el tamaño permitido. Reduce su tamaño e inténtalo nuevamente.")]
    [InlineData(503, "El servicio necesario no está disponible en este momento. Inténtalo nuevamente más tarde.")]
    public void ForStatus_returns_clear_message_for_http_failures(int statusCode, string expectedMessage)
    {
        Assert.Equal(expectedMessage, UserFacingErrorMessages.ForStatus(statusCode));
    }

    [Fact]
    public void ForException_does_not_return_internal_exception_details()
    {
        var message = UserFacingErrorMessages.ForException(
            new InvalidOperationException("SqlException: SELECT * FROM Payments failed."),
            409);

        Assert.Equal(
            "El registro cambió o la operación ya no está disponible. Actualiza la página y vuelve a intentarlo.",
            message);
        Assert.DoesNotContain("SqlException", message);
        Assert.DoesNotContain("SELECT", message);
    }

    [Fact]
    public void ForException_describes_expected_business_state_failures()
    {
        var exception = new InvalidOperationException("Only an uploaded receipt can be approved.");

        Assert.True(UserFacingErrorMessages.IsExpectedConflict(exception));
        Assert.Equal(
            "Solo puedes aprobar un comprobante que ya se haya cargado.",
            UserFacingErrorMessages.ForException(exception, 409));
    }

    [Fact]
    public void Unexpected_invalid_operation_is_not_misreported_as_a_business_conflict()
    {
        var exception = new InvalidOperationException("A successful result cannot have an error.");

        Assert.False(UserFacingErrorMessages.IsExpectedConflict(exception));
        Assert.Equal(
            "El servidor no pudo completar la solicitud. Inténtalo nuevamente; si el problema continúa, contacta con soporte.",
            UserFacingErrorMessages.ForException(exception, 500));
    }

    [Fact]
    public void ForModelValidation_names_the_field_that_needs_attention()
    {
        var message = UserFacingErrorMessages.ForModelValidation("request.Email", "The Email field is required.");

        Assert.Equal("Completa el campo correo electrónico.", message);
    }
}
