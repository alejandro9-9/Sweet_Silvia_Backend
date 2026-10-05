using System.Text.RegularExpressions;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Common;

public static class UserFacingErrorMessages
{
    private static readonly IReadOnlyDictionary<string, string> KnownMessages =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Address.InvalidProvince"] = "La provincia no corresponde al departamento seleccionado.",
            ["Address.DeliveryZoneUnavailable"] = "Aún no hay cobertura de envío para la dirección seleccionada.",
            ["Address.InvalidDistrict"] = "El distrito no corresponde a la provincia seleccionada.",
            ["Cart.InvalidQuantity"] = "La cantidad debe ser mayor que cero.",
            ["Cart.StockNotEnough"] = "No hay suficientes unidades disponibles. Ajusta la cantidad o elige otra variante.",
            ["Coupon.Invalid"] = "El cupón no existe, venció o no está disponible.",
            ["Coupon.InvalidScope"] = "Este cupón no aplica a los productos seleccionados.",
            ["Coupon.TotalUsesExceeded"] = "Este cupón alcanzó el límite total de usos.",
            ["Coupon.UserUsesExceeded"] = "Ya alcanzaste el máximo de usos permitido para este cupón.",
            ["Coupon.AlreadyApplied"] = "Este cupón ya está aplicado al carrito.",
            ["Coupon.CartItemRequired"] = "Selecciona los productos del carrito a los que quieres aplicar el cupón.",
            ["Coupon.InvalidQuantity"] = "La cantidad de productos indicada para el cupón no es válida.",
            ["Coupon.NotApplicable"] = "El cupón no cumple las condiciones de este pedido.",
            ["Coupon.NotAppliedToCart"] = "Aplica el cupón al carrito antes de confirmar el pedido.",
            ["Coupon.MultipleCouponsNotSupported"] = "Solo puedes usar un cupón por pedido.",
            ["Coupon.CodeAlreadyExists"] = "Ya existe un cupón con ese código.",
            ["GoogleAuth.NotConfigured"] = "El inicio de sesión con Google no está disponible en este momento.",
            ["Auth.Unauthorized"] = "Tu sesión no es válida. Inicia sesión nuevamente.",
            ["Auth.InvalidCredentials"] = "El correo electrónico o la contraseña son incorrectos.",
            ["Auth.UserBlocked"] = "Tu cuenta está bloqueada. Contacta con soporte para recuperar el acceso.",
            ["Auth.UserInactive"] = "Tu cuenta está inactiva. Contacta con soporte para recuperar el acceso.",
            ["Error.NullValue"] = "Falta completar un dato obligatorio.",
            ["User.GoogleLogin"] = "No se pudo iniciar sesión con Google. Verifica la cuenta e inténtalo nuevamente.",
            ["User.PasswordInvalid"] = "La contraseña actual no es correcta.",
            ["User.EmailAlreadyExists"] = "Ya existe una cuenta con ese correo electrónico.",
            ["User.LastAdministrator"] = "No puedes desactivar ni cambiar el rol del último administrador.",
            ["User.AvatarInvalid"] = "La imagen de perfil no es válida. Elige JPG, PNG o WEBP y respeta el tamaño permitido.",
            ["User.Login"] = "No se pudo iniciar sesión. Revisa tus datos e inténtalo nuevamente.",
            ["User.Logout"] = "No se pudo cerrar la sesión. Inténtalo nuevamente.",
            ["User.CannotBlockSelf"] = "No puedes bloquear tu propia cuenta.",
            ["User.CannotChangeOwnRole"] = "No puedes cambiar el rol de tu propia cuenta.",
            ["Role.Invalid"] = "El rol seleccionado no existe o no está disponible.",
            ["Role.NameAlreadyExists"] = "Ya existe un rol con ese nombre.",
            ["Shipping.CourierTypeMismatch"] = "El tipo de envío no coincide con el servicio seleccionado.",
            ["Shipping.InvalidDestination"] = "No hay cobertura de envío para el destino seleccionado.",
            ["Shipping.InvalidProvince"] = "La provincia no corresponde al departamento seleccionado.",
            ["Shipping.InvalidDistrict"] = "El distrito no corresponde a la provincia seleccionada.",
            ["Shipping.InvalidAgency"] = "La agencia de Olva no está disponible para el destino seleccionado.",
            ["Shipping.DestinationUbigeoMissing"] = "Falta el código de ubicación del destino. Verifica la dirección.",
            ["Shipping.PackageDimensionsInvalid"] = "Completa dimensiones y peso válidos para calcular el envío.",
            ["Shipping.InvalidRate"] = "La tarifa de envío seleccionada ya no está disponible. Actualiza las opciones.",
            ["Shipping.AddressRequired"] = "Selecciona una dirección de entrega para continuar.",
            ["Shipping.LocationRequired"] = "Completa el departamento, la provincia y el distrito de entrega.",
            ["Shipping.CourierUnavailable"] = "El servicio de envío seleccionado no está disponible para esta dirección.",
            ["Shipping.AgencyRequired"] = "Selecciona una agencia de Olva para continuar.",
            ["Shipping.QuoteChanged"] = "La tarifa de envío cambió. Actualiza la cotización antes de confirmar el pedido.",
            ["Shipment.OrderNotReady"] = "El pedido todavía no está listo para generar el envío.",
            ["Shipment.AgencyRequired"] = "Selecciona una agencia para registrar este envío.",
            ["Shipment.InvalidCourier"] = "El servicio de envío seleccionado no está disponible.",
            ["Shipment.AlreadyExists"] = "Este pedido ya tiene un envío registrado.",
            ["Shipment.InvalidCost"] = "El costo del envío no es válido.",
            ["Shipment.InvalidActor"] = "No tienes permiso para cambiar este envío.",
            ["Shipment.OrderNotPreparing"] = "Solo puedes registrar el envío cuando el pedido está en preparación.",
            ["Shipment.OrderNotShipped"] = "Solo puedes actualizar el envío cuando el pedido ya fue despachado.",
            ["Order.EmptyCart"] = "Tu carrito está vacío. Agrega productos antes de confirmar el pedido.",
            ["Order.InvalidCartItem"] = "Un producto del carrito cambió o ya no está disponible. Actualiza el carrito.",
            ["Order.UnsupportedCurrency"] = "La moneda del pedido no es compatible.",
            ["Order.PaymentConfirmationRequired"] = "Confirma el pago antes de continuar con el pedido.",
            ["Order.InvalidCancellation"] = "Este pedido ya no se puede cancelar en su estado actual.",
            ["Order.Empty"] = "No se puede completar un pedido sin productos.",
            ["Order.InvalidPaymentStatus"] = "El estado actual del pago no permite continuar.",
            ["Stock.NotEnough"] = "No hay suficientes unidades en stock. Actualiza la cantidad o elige otra variante.",
            ["Stock.ReservationMismatch"] = "La disponibilidad cambió mientras se procesaba el pedido. Actualiza el carrito.",
            ["Stock.Changed"] = "El stock cambió mientras editabas. Actualiza la página y vuelve a intentarlo.",
            ["Stock.InvalidAdjustment"] = "El ajuste de stock no es válido. Revisa la cantidad ingresada.",
            ["PaymentMethod.ExternalIntegrationRequired"] = "Este método de pago necesita una integración externa antes de estar disponible.",
            ["PaymentMethod.CashUnavailableForOlva"] = "El pago en efectivo no está disponible para los envíos por Olva.",
            ["Payment.ActivePaymentExists"] = "Ya existe un intento de pago activo para este pedido.",
            ["Payment.InvalidAmount"] = "El monto del pago no coincide con el total del pedido.",
            ["Payment.NotInReview"] = "Este pago no está pendiente de revisión.",
            ["PaymentReview.InvalidResult"] = "El resultado de la revisión del pago no es válido.",
            ["PaymentReceipt.InvalidAmount"] = "El comprobante no coincide con el monto del pedido.",
            ["Izipay.CheckoutDetailsRequired"] = "Completa los datos de contacto y el método de pago para iniciar el pago con Izipay.",
            ["Izipay.InvalidCustomer"] = "Los datos del titular no son válidos. Revísalos e inténtalo nuevamente.",
            ["Izipay.ActiveCheckoutInvalid"] = "El intento de pago venció o ya no es válido. Genera un nuevo intento.",
            ["Izipay.InvalidGateway"] = "El método de pago Izipay no está disponible para este pedido.",
            ["Izipay.MultipleActiveGatewayPayments"] = "Hay más de un intento de pago activo. Genera un nuevo intento antes de continuar.",
            ["Izipay.ActiveGatewayPaymentExists"] = "Ya existe un intento de pago activo. Continúa con ese intento o genera uno nuevo.",
            ["Izipay.InvalidSignature"] = "No se pudo validar la confirmación del pago. Contacta con soporte.",
            ["Izipay.InvalidPayload"] = "Izipay envió una confirmación de pago incompleta. Contacta con soporte.",
            ["Izipay.PaymentMismatch"] = "Los datos del pago no coinciden con el pedido.",
            ["Izipay.PaymentNotPending"] = "Este intento de pago ya no está pendiente.",
            ["Izipay.AmountMismatch"] = "El monto confirmado por Izipay no coincide con el total del pedido.",
            ["Izipay.NotAvailable"] = "Izipay no está disponible en este momento. Elige otro método de pago o inténtalo más tarde.",
            ["Olva.QuotePriceMissing"] = "Olva no devolvió una tarifa de envío válida. Actualiza la cotización o elige otra opción.",
            ["Product.InvalidOrder"] = "El orden de los productos no es válido. Actualiza la lista e inténtalo nuevamente.",
            ["ProductVariant.SkuAlreadyExists"] = "Ya existe una variante con ese SKU.",
            ["ProductImage.InvalidVariant"] = "La variante seleccionada no pertenece a este producto.",
            ["ProductImage.InvalidOrder"] = "El orden de las imágenes no es válido.",
        };

    private static readonly IReadOnlyDictionary<string, string> ResourceNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Address"] = "la dirección",
            ["Cart"] = "el carrito",
            ["CartItem"] = "el producto del carrito",
            ["Category"] = "la categoría",
            ["Collection"] = "la colección",
            ["Coupon"] = "el cupón",
            ["Courier"] = "el servicio de envío",
            ["Department"] = "el departamento",
            ["District"] = "el distrito",
            ["Order"] = "el pedido",
            ["Payment"] = "el pago",
            ["PaymentMethod"] = "el método de pago",
            ["PaymentReceipt"] = "el comprobante de pago",
            ["Product"] = "el producto",
            ["ProductImage"] = "la imagen del producto",
            ["ProductImages"] = "las imágenes del producto",
            ["ProductVariant"] = "la variante del producto",
            ["Province"] = "la provincia",
            ["Role"] = "el rol",
            ["Shipment"] = "el envío",
            ["ShippingRate"] = "la tarifa de envío",
            ["User"] = "la cuenta",
        };

    public static Error Localize(Error error)
    {
        var message = GetKnownMessage(error.Code)
            ?? GetNotFoundMessage(error.Code)
            ?? GetCategoryMessage(error.Code)
            ?? ForStatus(error.StatusCode ?? 400);

        return error with { Message = message };
    }

    public static string ForException(Exception exception, int statusCode, bool oversizedRequest = false)
    {
        if (oversizedRequest)
        {
            return ForStatus(413);
        }

        if (exception is ArgumentNullException argumentNullException)
        {
            var field = GetFieldLabel(argumentNullException.ParamName);
            return field is null ? "Falta completar un dato obligatorio." : $"Falta completar el campo {field}.";
        }

        if (exception is ArgumentException argumentException)
        {
            return LocalizeArgumentMessage(argumentException);
        }

        if (exception is TimeoutException)
        {
            return "El servicio tardó demasiado en responder. Inténtalo nuevamente.";
        }

        if (exception is HttpRequestException)
        {
            return "No pudimos conectar con un servicio necesario para completar la operación. Inténtalo nuevamente.";
        }

        if (exception is InvalidDataException)
        {
            return "El archivo no tiene un contenido válido. Elige otro archivo e inténtalo nuevamente.";
        }

        if (exception is IOException)
        {
            return "No se pudo leer o guardar el archivo. Verifica que sea válido e inténtalo nuevamente.";
        }

        if (exception is NotSupportedException)
        {
            return ForStatus(415);
        }

        if (exception is InvalidOperationException)
        {
            var message = exception.Message;
            if (message.Contains("not enough stock", StringComparison.OrdinalIgnoreCase))
            {
                return "No hay suficientes unidades disponibles para completar esta operación.";
            }

            if (message.Contains("cart item was not found", StringComparison.OrdinalIgnoreCase))
            {
                return "Ese producto ya no está en tu carrito. Actualiza el carrito e inténtalo nuevamente.";
            }

            if (message.Contains("cart is not active", StringComparison.OrdinalIgnoreCase))
            {
                return "El carrito ya no está activo. Actualiza la página para continuar con el carrito vigente.";
            }

            if (message.Contains("user is already linked to another google account", StringComparison.OrdinalIgnoreCase))
            {
                return "Esta cuenta ya está vinculada a otro perfil de Google.";
            }

            if (message.Contains("gateway payment cannot be settled with a manual receipt", StringComparison.OrdinalIgnoreCase))
            {
                return "Este pago con tarjeta debe confirmarse desde Izipay; no admite un comprobante manual.";
            }

            if (message.Contains("receipt can only be uploaded when payment is pending or rejected", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes subir un comprobante cuando el pago está pendiente o fue rechazado.";
            }

            if (message.Contains("uploaded receipt can be approved", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes aprobar un comprobante que ya se haya cargado.";
            }

            if (message.Contains("uploaded receipt can be rejected", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes rechazar un comprobante que ya se haya cargado.";
            }

            if (message.Contains("payment can only be approved when it is in review", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes aprobar pagos que estén pendientes de revisión.";
            }

            if (message.Contains("payment can only be rejected when it is in review", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes rechazar pagos que estén pendientes de revisión.";
            }

            if (message.Contains("cash payment can only be approved when it is pending", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes aprobar un pago en efectivo que esté pendiente.";
            }

            if (message.Contains("cash payment can only be rejected when it is pending", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes rechazar un pago en efectivo que esté pendiente.";
            }

            if (message.Contains("gateway payment can only be approved when it is pending", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes aprobar un pago de Izipay que esté pendiente.";
            }

            if (message.Contains("gateway payment can only be rejected when it is pending", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes rechazar un pago de Izipay que esté pendiente.";
            }

            if (message.Contains("only a pending gateway payment can require settlement review", StringComparison.OrdinalIgnoreCase))
            {
                return "Solo puedes enviar a revisión un pago de Izipay que esté pendiente.";
            }

            if (message.Contains("only an active gateway payment can refresh its session", StringComparison.OrdinalIgnoreCase))
            {
                return "Este intento de pago ya no está activo. Genera un nuevo intento para continuar.";
            }

            if (message.Contains("cannot be registered from", StringComparison.OrdinalIgnoreCase))
            {
                return "No se puede registrar el envío porque el pedido aún no está en preparación.";
            }

            if (message.Contains("shipment cannot change from", StringComparison.OrdinalIgnoreCase))
            {
                return "No se puede cambiar el envío desde su estado actual.";
            }

            if (message.Contains("order cannot change from", StringComparison.OrdinalIgnoreCase))
            {
                return "No se puede cambiar el pedido desde su estado actual.";
            }

            if (message.Contains("los roles del sistema no se pueden renombrar", StringComparison.OrdinalIgnoreCase))
            {
                return "Los roles del sistema no se pueden renombrar.";
            }

            if (message.Contains("los roles del sistema no se pueden desactivar", StringComparison.OrdinalIgnoreCase))
            {
                return "Los roles del sistema no se pueden desactivar.";
            }
        }

        if (statusCode == 409)
        {
            return "El registro cambió o la operación ya no está disponible. Actualiza la página y vuelve a intentarlo.";
        }

        return ForStatus(statusCode);
    }

    public static bool IsExpectedConflict(InvalidOperationException exception)
    {
        var message = exception.Message;
        return message.Contains("not enough stock", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cart item was not found", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cart is not active", StringComparison.OrdinalIgnoreCase)
            || message.Contains("already linked to another google account", StringComparison.OrdinalIgnoreCase)
            || message.Contains("gateway payment cannot be settled", StringComparison.OrdinalIgnoreCase)
            || message.Contains("receipt can only be uploaded", StringComparison.OrdinalIgnoreCase)
            || message.Contains("uploaded receipt can be", StringComparison.OrdinalIgnoreCase)
            || message.Contains("payment can only be", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cash payment can only be", StringComparison.OrdinalIgnoreCase)
            || message.Contains("gateway payment can only be", StringComparison.OrdinalIgnoreCase)
            || message.Contains("only a pending gateway payment can", StringComparison.OrdinalIgnoreCase)
            || message.Contains("only an active gateway payment can", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cannot be registered from", StringComparison.OrdinalIgnoreCase)
            || message.Contains("shipment cannot change from", StringComparison.OrdinalIgnoreCase)
            || message.Contains("order cannot change from", StringComparison.OrdinalIgnoreCase)
            || message.Contains("los roles del sistema no se pueden", StringComparison.OrdinalIgnoreCase);
    }

    public static string ForModelValidation(string field, string message)
    {
        var fieldLabel = GetFieldLabel(field) ?? "dato";
        var plainMessage = message.Trim();

        if (plainMessage.Contains("required", StringComparison.OrdinalIgnoreCase))
        {
            return $"Completa el campo {fieldLabel}.";
        }

        var maximumLength = Regex.Match(plainMessage, @"maximum length of \D*(\d+)", RegexOptions.IgnoreCase);
        if (maximumLength.Success)
        {
            return $"El campo {fieldLabel} no debe superar {maximumLength.Groups[1].Value} caracteres.";
        }

        if (plainMessage.Contains("not valid", StringComparison.OrdinalIgnoreCase)
            || plainMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase))
        {
            return $"El valor ingresado para {fieldLabel} no es válido.";
        }

        return $"Revisa el campo {fieldLabel}; el valor ingresado no es válido.";
    }

    public static string ForStatus(int statusCode) => statusCode switch
    {
        400 => "Hay datos incorrectos o incompletos. Revísalos y vuelve a intentarlo.",
        401 => "Tu sesión venció o ya no es válida. Inicia sesión nuevamente.",
        403 => "No tienes permiso para realizar esta acción.",
        404 => "No encontramos lo solicitado. Actualiza la página e inténtalo nuevamente.",
        409 => "La información cambió o la operación ya no está disponible. Actualiza la página e inténtalo nuevamente.",
        413 => "El archivo supera el tamaño permitido. Reduce su tamaño e inténtalo nuevamente.",
        415 => "El formato del archivo no es compatible. Elige un formato permitido.",
        422 => "Algunos datos necesitan corrección. Revisa los campos indicados.",
        429 => "Hay demasiadas solicitudes. Espera un momento y vuelve a intentarlo.",
        503 => "El servicio necesario no está disponible en este momento. Inténtalo nuevamente más tarde.",
        >= 500 => "El servidor no pudo completar la solicitud. Inténtalo nuevamente; si el problema continúa, contacta con soporte.",
        _ => "No se pudo completar la solicitud. Revisa los datos e inténtalo nuevamente."
    };

    public static string GetTitle(int statusCode) => statusCode switch
    {
        400 or 422 => "Revisa los datos enviados",
        401 => "Inicia sesión nuevamente",
        403 => "Acceso no permitido",
        404 => "No encontramos lo solicitado",
        409 => "La información cambió",
        413 => "El archivo es demasiado grande",
        415 => "Formato no compatible",
        429 => "Espera un momento",
        503 => "Servicio temporalmente no disponible",
        >= 500 => "No se pudo completar la solicitud",
        _ => "No se pudo completar la solicitud"
    };

    private static string? GetKnownMessage(string code)
        => KnownMessages.TryGetValue(code, out var message) ? message : null;

    private static string? GetNotFoundMessage(string code)
    {
        const string suffix = ".NotFound";
        if (!code.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var resource = code[..^suffix.Length];
        return ResourceNames.TryGetValue(resource, out var resourceName)
            ? $"No encontramos {resourceName}."
            : "No encontramos el elemento solicitado. Actualiza la página e inténtalo nuevamente.";
    }

    private static string? GetCategoryMessage(string code)
    {
        var separator = code.IndexOf('.');
        var category = separator < 0 ? code : code[..separator];

        return category.ToUpperInvariant() switch
        {
            "ADDRESS" => "No se pudo guardar la dirección. Revisa los datos de entrega.",
            "AUTH" => "No se pudo validar tu acceso. Inicia sesión nuevamente.",
            "CART" => "No se pudo actualizar el carrito. Revisa las cantidades y el stock disponible.",
            "COUPON" => "No se pudo aplicar el cupón. Revisa su vigencia y las condiciones del pedido.",
            "GOOGLEAUTH" => "No se pudo iniciar sesión con Google. Inténtalo nuevamente.",
            "IZIPAY" => "Izipay no pudo procesar el pago. Genera un nuevo intento o elige otro método.",
            "OLVA" => "Olva no pudo calcular el envío. Verifica la dirección y vuelve a cotizar.",
            "ORDER" => "No se pudo procesar el pedido. Revisa el carrito, el envío y el pago.",
            "PAYMENT" or "PAYMENTMETHOD" or "PAYMENTRECEIPT" or "PAYMENTREVIEW" => "No se pudo procesar el pago. Revisa el método y los datos del pedido.",
            "PRODUCT" or "PRODUCTIMAGE" or "PRODUCTVARIANT" or "CATEGORY" or "COLLECTION" => "No se pudo guardar el producto. Revisa los datos e inténtalo nuevamente.",
            "ROLE" or "USER" => "No se pudo actualizar la cuenta. Revisa el usuario y el rol seleccionado.",
            "SHIPMENT" or "SHIPPING" or "SHIPPINGRATE" or "COURIER" => "No se pudo procesar el envío. Revisa la dirección y el servicio seleccionado.",
            "STOCK" => "No se pudo actualizar el stock. Revisa la disponibilidad e inténtalo nuevamente.",
            _ => null
        };
    }

    private static string LocalizeArgumentMessage(ArgumentException exception)
    {
        var message = exception.Message;
        var field = GetFieldLabel(exception.ParamName);

        if (message.Contains("greater than zero", StringComparison.OrdinalIgnoreCase))
        {
            return field is null ? "La cantidad debe ser mayor que cero." : $"El campo {field} debe ser mayor que cero.";
        }

        if (message.Contains("is required", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cannot be null", StringComparison.OrdinalIgnoreCase))
        {
            return field is null ? "Falta completar un dato obligatorio." : $"Completa el campo {field}.";
        }

        if (message.Contains("cannot exceed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cannot be longer than", StringComparison.OrdinalIgnoreCase))
        {
            return field is null ? "Uno de los datos supera el límite permitido." : $"El campo {field} supera el límite permitido.";
        }

        if (message.Contains("invalid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not valid", StringComparison.OrdinalIgnoreCase))
        {
            return field is null ? "Uno de los datos ingresados no es válido." : $"El valor ingresado para {field} no es válido.";
        }

        return "Hay datos incorrectos o incompletos. Revisa el formulario e inténtalo nuevamente.";
    }

    private static string? GetFieldLabel(string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return null;
        }

        var name = field.Split('.').Last().Split('[')[0];
        var knownFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AddressId"] = "dirección",
            ["Amount"] = "monto",
            ["Color"] = "color",
            ["CouponCode"] = "código del cupón",
            ["DepartmentId"] = "departamento",
            ["DistrictId"] = "distrito",
            ["Email"] = "correo electrónico",
            ["File"] = "archivo",
            ["Name"] = "nombre",
            ["OrderId"] = "pedido",
            ["Password"] = "contraseña",
            ["PaymentId"] = "pago",
            ["Phone"] = "teléfono",
            ["ProductId"] = "producto",
            ["ProductVariantId"] = "variante del producto",
            ["ProvinceId"] = "provincia",
            ["Quantity"] = "cantidad",
            ["ReceiverName"] = "nombre de quien recibe",
            ["ReceiverPhone"] = "teléfono de quien recibe",
            ["Size"] = "talla",
            ["Sku"] = "el SKU",
            ["Url"] = "dirección web",
        };

        if (knownFields.TryGetValue(name, out var label))
        {
            return label;
        }

        return Regex.Replace(name, "([a-z0-9])([A-Z])", "$1 $2").ToLowerInvariant();
    }
}
