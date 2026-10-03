using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Payments;
using Sweet_Silvia_Application.Abstractions.Orders;
using Sweet_Silvia_Application.Izipay;
using Sweet_Silvia_Application.Order;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Cart;
using Sweet_Silvia_Domain.CartItem;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.OrderItem;
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.PaymentMethod;
using Sweet_Silvia_Domain.ProductVariant;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using Sweet_Silvia_Domain.StockMovement;
using Sweet_Silvia_Infrastructure.Abstractions.Payments;

namespace Sweet_Silvia_Tests;

public sealed class IzipayIntegrationTests
{
    [Theory]
    [InlineData("cliente@sweetsilvia.local")]
    [InlineData("usuario@localhost")]
    [InlineData("correo-invalido")]
    public void CustomerValidation_rejects_local_or_invalid_email_addresses(string email)
    {
        var customer = new IzipayCustomerDetails(
            "Ana", "Prueba", email, "999123456", "Av. Lima 123", "15001", "Lima", "Lima");

        Assert.Equal(IzipayCustomerValidation.InvalidEmailMessage, IzipayCustomerValidation.Validate(customer));
    }

    [Fact]
    public void CustomerValidation_accepts_a_public_email_address()
    {
        var customer = new IzipayCustomerDetails(
            "Ana", "Prueba", "ana@example.com", "999123456", "Av. Lima 123", "15001", "Lima", "Lima");

        Assert.Null(IzipayCustomerValidation.Validate(customer));
    }

    [Fact]
    public async Task Izipay_notification_does_not_decrease_stock_when_payment_was_not_paid()
    {
        var fixture = CreateNotificationFixture("UNPAID", "CLOSED");

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Rejected, fixture.Payment.Status);
        Assert.Equal(5, fixture.Variant.PhysicalStock);
        Assert.Equal(5, Assert.Single(fixture.CartItems.Query()).Quantity);
        Assert.Empty(fixture.StockMovements.Query());
    }

    [Fact]
    public async Task Izipay_paid_notification_decreases_grouped_stock_once_even_when_retried()
    {
        var fixture = CreateNotificationFixture("PAID", "CLOSED");

        var firstResult = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);
        var retryResult = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(retryResult.IsSuccess);
        Assert.Equal(PaymentStatus.Approved, fixture.Payment.Status);
        Assert.Equal(OrderStatus.Paid, fixture.Order.Status);
        Assert.Equal(2, fixture.Variant.PhysicalStock);
        Assert.Equal(2, Assert.Single(fixture.CartItems.Query()).Quantity);
        var sales = fixture.StockMovements.Query().Where(movement => movement.Type == StockMovementType.Sale).ToList();
        var sale = Assert.Single(sales);
        Assert.Equal(3, sale.Quantity);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Customer_orders_include_gateway_orders_while_payment_is_pending()
    {
        var userId = Guid.NewGuid();
        var pendingOrder = CreateOrder(userId);
        var anotherUserOrder = CreateOrder(Guid.NewGuid());
        var handler = new GetOrdersByUserQueryHandler(
            new MemoryRepository<Order>(new[] { pendingOrder, anotherUserOrder }));

        var result = await handler.Handle(new GetOrdersByUserQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(pendingOrder.Id, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task Explicit_form_retry_refreshes_the_existing_active_payment_without_adding_another_attempt()
    {
        var order = CreateOrder(Guid.NewGuid());
        var paymentMethod = PaymentMethod.Create("Izipay", PaymentMethodType.Gateway, false, true);
        const string activeReference = "existing-izipay-reference";
        var activePayment = Payment.CreateGatewayPending(
            order.Id, paymentMethod.Id, Money.Create(30m), activeReference, activeReference, DateTimeOffset.UtcNow.AddHours(1), "FORM_TOKEN_CREATED");
        var payments = new MemoryRepository<Payment>(new[] { activePayment });
        var gateway = new SuccessfulPaymentGateway();
        var unitOfWork = new CountingUnitOfWork();
        var handler = new GenerateIzipayPaymentFormCommandHandler(
            payments,
            new MemoryRepository<Order>(new[] { order }),
            new MemoryRepository<PaymentMethod>(new[] { paymentMethod }),
            gateway,
            unitOfWork);

        var result = await handler.Handle(new GenerateIzipayPaymentFormCommand(
            order.Id,
            paymentMethod.Id,
            new IzipayCustomerDetails("Ana", "Prueba", "ana@example.com", "999123456", "Av. Lima 123", "15001", "Lima", "Lima"),
            ReplaceActivePayment: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(activePayment.Id, result.Value.PaymentId);
        Assert.Equal(activeReference, result.Value.OrderReference);
        Assert.Equal(activeReference, gateway.Request?.OrderReference);
        Assert.Single(payments.Query());
        Assert.Equal(PaymentStatus.PendingGateway, activePayment.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Repeated_checkout_reuses_the_existing_order_and_stock_reservation()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);
        var paymentMethod = PaymentMethod.Create("Izipay", PaymentMethodType.Gateway, false, true);
        const string activeReference = "existing-checkout-reference";
        var payment = Payment.CreateGatewayPending(
            order.Id, paymentMethod.Id, order.Total, activeReference, activeReference, DateTimeOffset.UtcNow.AddHours(1), "FORM_TOKEN_CREATED");
        var orders = new MemoryRepository<Order>(new[] { order });
        var payments = new MemoryRepository<Payment>(new[] { payment });
        var gateway = new SuccessfulPaymentGateway();
        var unitOfWork = new CountingUnitOfWork();
        var handler = new CreateOrderCommandHandler(
            orders,
            new MemoryRepository<Sweet_Silvia_Domain.OrderItem.OrderItem>(),
            new MemoryRepository<Sweet_Silvia_Domain.Address.Address>(),
            new MemoryRepository<Cart>(),
            new MemoryRepository<CartItem>(),
            new MemoryRepository<Sweet_Silvia_Domain.CartAppliedCoupon.CartAppliedCoupon>(),
            new MemoryRepository<Sweet_Silvia_Domain.Coupon.Coupon>(),
            new MemoryRepository<Sweet_Silvia_Domain.CouponRestriction.CouponRestriction>(),
            new MemoryRepository<Sweet_Silvia_Domain.CouponUsage.CouponUsage>(),
            new MemoryRepository<Sweet_Silvia_Domain.Product.Product>(),
            new MemoryRepository<ProductVariant>(),
            new MemoryRepository<Sweet_Silvia_Domain.Department.Department>(),
            new MemoryRepository<Sweet_Silvia_Domain.Province.Province>(),
            new MemoryRepository<Sweet_Silvia_Domain.District.District>(),
            new MemoryRepository<Sweet_Silvia_Domain.Courier.Courier>(),
            new MemoryRepository<Sweet_Silvia_Domain.ShippingRate.ShippingRate>(),
            new MemoryRepository<Sweet_Silvia_Domain.OlvaAgency.OlvaAgency>(),
            new MemoryRepository<StockMovement>(),
            payments,
            new MemoryRepository<PaymentMethod>(new[] { paymentMethod }),
            null!,
            gateway,
            new NoOpUserCheckoutLock(),
            new NoOpCouponUsageLock(),
            unitOfWork);
        var customer = new IzipayCustomerDetails("Ana", "Prueba", "ana@example.com", "999123456", "Av. Lima 123", "15001", "Lima", "Lima");

        var result = await handler.Handle(new CreateOrderCommand(
            userId,
            AddressId: null,
            ShippingRateId: null,
            IzipayPaymentMethodId: paymentMethod.Id,
            IzipayCustomer: customer), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(order.Id, result.Value.Id);
        Assert.Equal(payment.Id, result.Value.IzipayPaymentForm?.PaymentId);
        Assert.Equal(activeReference, gateway.Request?.OrderReference);
        Assert.Single(orders.Query());
        Assert.Single(payments.Query());
        Assert.Equal(PaymentStatus.PendingGateway, payment.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static Order CreateOrder(Guid userId) => Order.Create(
        userId, null, null, null, null,
        Money.Create(30m), Money.Zero(), Money.Zero(), Money.Create(30m));

    private static NotificationFixture CreateNotificationFixture(string status, string cycle)
    {
        var variant = ProductVariant.Create(Guid.NewGuid(), "M", "Negro", Sku.Create("SKU-IZIPAY-1"), 5);
        var userId = Guid.NewGuid();
        var order = Order.Create(
            userId, null, null, null, null,
            Money.Create(30m), Money.Zero(), Money.Zero(), Money.Create(30m));
        var cart = Cart.Create(userId);
        var cartItems = new MemoryRepository<CartItem>(new[]
        {
            CartItem.Create(cart.Id, variant.Id, 5)
        });
        var orderItems = new MemoryRepository<OrderItem>(new[]
        {
            OrderItem.Create(order.Id, variant.Id, "Polo", "SKU-IZIPAY-1", "M", "Negro", 2, Money.Create(10m), Money.Zero(), Money.Create(20m)),
            OrderItem.Create(order.Id, variant.Id, "Polo", "SKU-IZIPAY-1", "M", "Negro", 1, Money.Create(10m), Money.Zero(), Money.Create(10m))
        });
        const string reference = "izipay-order-reference";
        var payment = Payment.CreateGatewayPending(
            order.Id, Guid.NewGuid(), Money.Create(30m), reference, reference, DateTimeOffset.UtcNow.AddHours(1), "FORM_TOKEN_CREATED");
        var answer = JsonSerializer.Serialize(new
        {
            shopId = "test-shop",
            orderStatus = status,
            orderCycle = cycle,
            orderDetails = new { orderId = reference, orderTotalAmount = 3000, orderCurrency = "PEN" }
        });

        var paymentRepository = new MemoryRepository<Payment>(new[] { payment });
        var orderRepository = new MemoryRepository<Order>(new[] { order });
        var variantRepository = new MemoryRepository<ProductVariant>(new[] { variant });
        var stockMovementRepository = new MemoryRepository<StockMovement>();
        var unitOfWork = new CountingUnitOfWork();
        var handler = new ProcessIzipayNotificationCommandHandler(
            new AcceptingSignatureValidator(),
            paymentRepository,
            orderRepository,
            orderItems,
            new MemoryRepository<Cart>(new[] { cart }),
            cartItems,
            variantRepository,
            stockMovementRepository,
            new MemoryRepository<Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory>(),
            new MemoryRepository<Sweet_Silvia_Domain.Coupon.Coupon>(),
            new MemoryRepository<Sweet_Silvia_Domain.CouponUsage.CouponUsage>(),
            new NoOpCouponUsageLock(),
            unitOfWork);

        return new NotificationFixture(
            handler,
            new ProcessIzipayNotificationCommand(answer, "test-signature", "sha256_hmac", "test-shop"),
            payment,
            order,
            variant,
            cartItems,
            stockMovementRepository,
            unitOfWork);
    }

    [Fact]
    public void SignatureValidator_uses_hmac_key_for_client_confirmation_and_server_notification()
    {
        const string password = "sandbox-rest-secret";
        const string hmacKey = "sandbox-hmac-secret";
        const string answer = "{\"orderStatus\":\"PAID\"}";
        using var clientHmac = new HMACSHA256(Encoding.UTF8.GetBytes(hmacKey));
        using var passwordHmac = new HMACSHA256(Encoding.UTF8.GetBytes(password));
        var clientSignature = Convert.ToHexString(clientHmac.ComputeHash(Encoding.UTF8.GetBytes(answer)));
        var passwordSignature = Convert.ToHexString(passwordHmac.ComputeHash(Encoding.UTF8.GetBytes(answer)));
        var validator = new IzipaySignatureValidator(new IzipayOptions
        {
            Password = password,
            HmacSha256Key = hmacKey
        });

        Assert.True(validator.IsClientConfirmationValid(answer, clientSignature));
        Assert.True(validator.IsNotificationValid(answer, clientSignature));
        Assert.False(validator.IsClientConfirmationValid(answer, passwordSignature));
        Assert.False(validator.IsNotificationValid(answer, passwordSignature));
        Assert.False(validator.IsClientConfirmationValid(answer + " ", clientSignature));
        Assert.False(validator.IsClientConfirmationValid(answer, Convert.ToBase64String(Convert.FromHexString(clientSignature))));
    }

    [Fact]
    public async Task PaymentForm_uses_basic_auth_minor_units_and_returns_form_token()
    {
        var handler = new CapturingHandler("{\"status\":\"SUCCESS\",\"answer\":{\"formToken\":\"sandbox-form-token\"}}");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.micuentaweb.pe/api-payment/")
        };
        var gateway = new IzipayPaymentGateway(httpClient, new IzipayOptions
        {
            ShopId = "12345678",
            Password = "sandbox-password",
            PublicKey = "test-public-key",
            NotificationUrl = "https://api.example.com/api/payments/izipay/notifications"
        });

        var result = await gateway.CreatePaymentFormAsync(new IzipayPaymentFormGatewayRequest(
            "order-reference-123",
            300.50m,
            "PEN",
            new IzipayCustomerDetails("Ana", "Prueba", "ana@example.com", "999123456", "Av. Lima 123", "15001", "Lima", "Lima")));

        Assert.True(result.IsSuccess);
        Assert.Equal("sandbox-form-token", result.Value.FormToken);
        Assert.Equal("12345678:test-public-key", result.Value.PublicKey);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.micuentaweb.pe/api-payment/V4/Charge/CreatePayment", handler.RequestUri?.ToString());

        var expectedAuthorization = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678:sandbox-password"));
        Assert.Equal("Basic", handler.Authorization?.Scheme);
        Assert.Equal(expectedAuthorization, handler.Authorization?.Parameter);

        using var body = JsonDocument.Parse(handler.Body!);
        var root = body.RootElement;
        Assert.Equal(30050, root.GetProperty("amount").GetInt32());
        Assert.Equal("PEN", root.GetProperty("currency").GetString());
        Assert.Equal("PAYMENT", root.GetProperty("formAction").GetString());
        Assert.Equal("order-reference-123", root.GetProperty("orderId").GetString());
        Assert.Equal("https://api.example.com/api/payments/izipay/notifications", root.GetProperty("ipnTargetUrl").GetString());
    }

    [Fact]
    public async Task PaymentForm_rejects_amounts_with_more_than_two_decimal_places()
    {
        var handler = new CapturingHandler("{\"status\":\"SUCCESS\",\"answer\":{\"formToken\":\"token\"}}");
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.micuentaweb.pe/api-payment/") };
        var gateway = new IzipayPaymentGateway(httpClient, new IzipayOptions
        {
            ShopId = "12345678",
            Password = "sandbox-password",
            PublicKey = "test-public-key"
        });

        var result = await gateway.CreatePaymentFormAsync(new IzipayPaymentFormGatewayRequest(
            "order-reference-123",
            10.001m,
            "PEN",
            new IzipayCustomerDetails("Ana", "Prueba", "ana@example.com", "", "", "", "Lima", "Lima")));

        Assert.True(result.IsFailure);
        Assert.Null(handler.Method);
    }

    private sealed class CapturingHandler(string responseBody) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public AuthenticationHeaderValue? Authorization { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed record NotificationFixture(
        ProcessIzipayNotificationCommandHandler Handler,
        ProcessIzipayNotificationCommand Command,
        Payment Payment,
        Order Order,
        ProductVariant Variant,
        MemoryRepository<CartItem> CartItems,
        MemoryRepository<StockMovement> StockMovements,
        CountingUnitOfWork UnitOfWork);

    private sealed class AcceptingSignatureValidator : IIzipaySignatureValidator
    {
        public bool IsClientConfirmationValid(string answer, string signature) => true;
        public bool IsNotificationValid(string answer, string signature) => true;
    }

    private sealed class SuccessfulPaymentGateway : IIzipayPaymentGateway
    {
        public IzipayPaymentFormGatewayRequest? Request { get; private set; }

        public Task<Result<IzipayPaymentFormGatewayResponse>> CreatePaymentFormAsync(
            IzipayPaymentFormGatewayRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(Result.Success(new IzipayPaymentFormGatewayResponse("fresh-form-token", "test-public-key")));
        }
    }

    private sealed class CountingUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class NoOpUserCheckoutLock : IUserCheckoutLock
    {
        public Task<IAsyncDisposable> AcquireAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IAsyncDisposable>(new EmptyAsyncDisposable());

        private sealed class EmptyAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    private sealed class NoOpCouponUsageLock : Sweet_Silvia_Application.Abstractions.Coupons.ICouponUsageLock
    {
        public Task<IAsyncDisposable> AcquireAsync(Guid couponId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IAsyncDisposable>(new EmptyAsyncDisposable());

        private sealed class EmptyAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    private sealed class MemoryRepository<TEntity>(IEnumerable<TEntity>? entities = null) : IRepository<TEntity>
        where TEntity : Entity
    {
        private readonly List<TEntity> _entities = entities?.ToList() ?? [];

        public IQueryable<TEntity> Query() => _entities.AsQueryable();

        public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_entities.FirstOrDefault(entity => entity.Id == id));

        public void Add(TEntity entity) => _entities.Add(entity);
        public void Update(TEntity entity) { }
        public void Remove(TEntity entity) => _entities.Remove(entity);
    }
}
