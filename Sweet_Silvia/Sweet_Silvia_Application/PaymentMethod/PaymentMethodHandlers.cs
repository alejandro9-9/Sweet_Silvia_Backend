using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Domain.Abstractions;
using PaymentMethodEntity = Sweet_Silvia_Domain.PaymentMethod.PaymentMethod;

namespace Sweet_Silvia_Application.PaymentMethod;

internal sealed class CreatePaymentMethodCommandHandler(IRepository<PaymentMethodEntity> paymentMethods, IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePaymentMethodCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var paymentMethod = PaymentMethodEntity.Create(request.Name, request.Type, request.RequiresManualVerification, request.RequiresExternalIntegration);
        paymentMethods.Add(paymentMethod);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return paymentMethod.Id;
    }
}

internal sealed class UpdatePaymentMethodCommandHandler(IRepository<PaymentMethodEntity> paymentMethods, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePaymentMethodCommand>
{
    public async Task<Result> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var paymentMethod = await paymentMethods.GetByIdAsync(request.Id, cancellationToken);
        if (paymentMethod is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("PaymentMethod", request.Id));
        }

        paymentMethod.Update(request.Name, request.Type, request.RequiresManualVerification, request.RequiresExternalIntegration);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeletePaymentMethodCommandHandler(IRepository<PaymentMethodEntity> paymentMethods, IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePaymentMethodCommand>
{
    public async Task<Result> Handle(DeletePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var paymentMethod = await paymentMethods.GetByIdAsync(request.Id, cancellationToken);
        if (paymentMethod is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("PaymentMethod", request.Id));
        }

        paymentMethod.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetPaymentMethodByIdQueryHandler(IRepository<PaymentMethodEntity> paymentMethods)
    : IQueryHandler<GetPaymentMethodByIdQuery, PaymentMethodResponse>
{
    public Task<Result<PaymentMethodResponse>> Handle(GetPaymentMethodByIdQuery request, CancellationToken cancellationToken)
    {
        var paymentMethod = paymentMethods.Query().FirstOrDefault(paymentMethod => paymentMethod.Id == request.Id);
        return Task.FromResult(paymentMethod is null
            ? Result.Failure<PaymentMethodResponse>(ApplicationErrors.NotFound("PaymentMethod", request.Id))
            : Result.Success(ToResponse(paymentMethod)));
    }

    internal static PaymentMethodResponse ToResponse(PaymentMethodEntity paymentMethod)
    {
        return new PaymentMethodResponse(
            paymentMethod.Id,
            paymentMethod.Name,
            paymentMethod.Type,
            paymentMethod.RequiresManualVerification,
            paymentMethod.RequiresExternalIntegration,
            paymentMethod.IsActive);
    }
}

internal sealed class GetPaymentMethodsQueryHandler(IRepository<PaymentMethodEntity> paymentMethods)
    : IQueryHandler<GetPaymentMethodsQuery, List<PaymentMethodResponse>>
{
    public Task<Result<List<PaymentMethodResponse>>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var query = paymentMethods.Query();
        if (request.OnlyActive)
        {
            query = query.Where(paymentMethod => paymentMethod.IsActive);
        }

        return Task.FromResult(Result.Success(query.Select(GetPaymentMethodByIdQueryHandler.ToResponse).ToList()));
    }
}
