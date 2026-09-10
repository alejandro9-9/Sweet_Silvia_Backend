using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Collection;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.Product;
using Sweet_Silvia_Application.ProductImage;
using Sweet_Silvia_Application.ProductVariant;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using CategoryEntity = Sweet_Silvia_Domain.Category.Category;
using CollectionEntity = Sweet_Silvia_Domain.Collection.Collection;
using ProductEntity = Sweet_Silvia_Domain.Product.Product;
using ProductImageEntity = Sweet_Silvia_Domain.ProductImage.ProductImage;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;

namespace Sweet_Silvia_Application.Category;

internal sealed class CreateCategoryCommandHandler(IRepository<CategoryEntity> categories, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = CategoryEntity.Create(request.Name, request.Description);
        categories.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}

internal sealed class UpdateCategoryCommandHandler(IRepository<CategoryEntity> categories, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Category", request.Id));
        }

        category.Update(request.Name, request.Description);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteCategoryCommandHandler(IRepository<CategoryEntity> categories, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Category", request.Id));
        }

        category.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetCategoryByIdQueryHandler(IRepository<CategoryEntity> categories)
    : IQueryHandler<GetCategoryByIdQuery, CategoryResponse>
{
    public Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = categories.Query().FirstOrDefault(category => category.Id == request.Id && category.IsActive);
        return Task.FromResult(category is null
            ? Result.Failure<CategoryResponse>(ApplicationErrors.NotFound("Category", request.Id))
            : Result.Success(new CategoryResponse(category.Id, category.Name, category.Description, category.IsActive)));
    }
}

internal sealed class GetCategoriesQueryHandler(IRepository<CategoryEntity> categories)
    : IQueryHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    public Task<Result<List<CategoryResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = request.OnlyActive ? categories.Query().Where(category => category.IsActive) : categories.Query();
        return Task.FromResult(Result.Success(query.Select(category => new CategoryResponse(category.Id, category.Name, category.Description, category.IsActive)).ToList()));
    }
}

internal sealed class CreateCollectionCommandHandler(IRepository<CollectionEntity> collections, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCollectionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = CollectionEntity.Create(request.Name, request.Description);
        collections.Add(collection);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return collection.Id;
    }
}

internal sealed class UpdateCollectionCommandHandler(IRepository<CollectionEntity> collections, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCollectionCommand>
{
    public async Task<Result> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await collections.GetByIdAsync(request.Id, cancellationToken);
        if (collection is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Collection", request.Id));
        }

        collection.Update(request.Name, request.Description);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteCollectionCommandHandler(IRepository<CollectionEntity> collections, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCollectionCommand>
{
    public async Task<Result> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await collections.GetByIdAsync(request.Id, cancellationToken);
        if (collection is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Collection", request.Id));
        }

        collection.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetCollectionByIdQueryHandler(IRepository<CollectionEntity> collections)
    : IQueryHandler<GetCollectionByIdQuery, CollectionResponse>
{
    public Task<Result<CollectionResponse>> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
    {
        var collection = collections.Query().FirstOrDefault(collection => collection.Id == request.Id && collection.IsActive);
        return Task.FromResult(collection is null
            ? Result.Failure<CollectionResponse>(ApplicationErrors.NotFound("Collection", request.Id))
            : Result.Success(new CollectionResponse(collection.Id, collection.Name, collection.Description, collection.IsActive)));
    }
}

internal sealed class GetCollectionsQueryHandler(IRepository<CollectionEntity> collections)
    : IQueryHandler<GetCollectionsQuery, List<CollectionResponse>>
{
    public Task<Result<List<CollectionResponse>>> Handle(GetCollectionsQuery request, CancellationToken cancellationToken)
    {
        var query = request.OnlyActive ? collections.Query().Where(collection => collection.IsActive) : collections.Query();
        return Task.FromResult(Result.Success(query.Select(collection => new CollectionResponse(collection.Id, collection.Name, collection.Description, collection.IsActive)).ToList()));
    }
}

internal sealed class CreateProductCommandHandler(
    IRepository<ProductEntity> products,
    IRepository<CategoryEntity> categories,
    IRepository<CollectionEntity> collections,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validation = CatalogMappings.ValidateProductCatalog(request.CategoryId, request.CollectionId, categories, collections);
        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Error);
        }

        var product = ProductEntity.Create(
            request.CategoryId,
            request.CollectionId,
            request.Name,
            request.Description,
            Money.Create(request.BasePrice, request.Currency));

        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}

internal sealed class UpdateProductCommandHandler(
    IRepository<ProductEntity> products,
    IRepository<CategoryEntity> categories,
    IRepository<CollectionEntity> collections,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Product", request.Id));
        }

        var validation = CatalogMappings.ValidateProductCatalog(request.CategoryId, request.CollectionId, categories, collections);
        if (validation.IsFailure)
        {
            return validation;
        }

        product.Update(
            request.Name,
            request.Description,
            Money.Create(request.BasePrice, request.Currency),
            request.CategoryId,
            request.CollectionId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteProductCommandHandler(IRepository<ProductEntity> products, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Product", request.Id));
        }

        product.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetProductByIdQueryHandler(IRepository<ProductEntity> products)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = products.Query().FirstOrDefault(product => product.Id == request.Id && product.IsActive);
        return Task.FromResult(product is null
            ? Result.Failure<ProductResponse>(ApplicationErrors.NotFound("Product", request.Id))
            : Result.Success(CatalogMappings.ToResponse(product)));
    }
}

internal sealed class GetProductsQueryHandler(
    IRepository<ProductEntity> products,
    IRepository<CategoryEntity> categories,
    IRepository<CollectionEntity> collections)
    : IQueryHandler<GetProductsQuery, List<ProductResponse>>
{
    public Task<Result<List<ProductResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = products.Query()
            .Where(product =>
                (!request.OnlyActive || product.IsActive) &&
                categories.Query().Any(category => category.Id == product.CategoryId && category.IsActive) &&
                (product.CollectionId == null || collections.Query().Any(collection => collection.Id == product.CollectionId && collection.IsActive)));

        if (request.CategoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == request.CategoryId);
        }

        if (request.CollectionId.HasValue)
        {
            query = query.Where(product => product.CollectionId == request.CollectionId);
        }

        return Task.FromResult(Result.Success(query.Select(CatalogMappings.ToResponse).ToList()));
    }
}

internal sealed class CreateProductVariantCommandHandler(
    IRepository<ProductEntity> products,
    IRepository<ProductVariantEntity> variants,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductVariantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        if (!products.Query().Any(product => product.Id == request.ProductId && product.IsActive))
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Product", request.ProductId));
        }

        var sku = Sku.Create(request.Sku);
        if (variants.Query().Any(variant => variant.Sku == sku))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("ProductVariant.SkuAlreadyExists", "SKU already exists."));
        }

        var variant = ProductVariantEntity.Create(
            request.ProductId,
            request.Size,
            request.Color,
            sku,
            request.PhysicalStock,
            request.Price.HasValue ? Money.Create(request.Price.Value, request.Currency) : null,
            request.ShippingWeightKg,
            request.ShippingLengthCm,
            request.ShippingWidthCm,
            request.ShippingHeightCm);

        variants.Add(variant);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return variant.Id;
    }
}

internal sealed class UpdateProductVariantCommandHandler(IRepository<ProductVariantEntity> variants, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductVariantCommand>
{
    public async Task<Result> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await variants.GetByIdAsync(request.Id, cancellationToken);
        if (variant is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ProductVariant", request.Id));
        }

        variant.Update(
            request.Size,
            request.Color,
            request.Price.HasValue ? Money.Create(request.Price.Value, request.Currency) : null,
            request.ShippingWeightKg,
            request.ShippingLengthCm,
            request.ShippingWidthCm,
            request.ShippingHeightCm);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteProductVariantCommandHandler(IRepository<ProductVariantEntity> variants, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductVariantCommand>
{
    public async Task<Result> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await variants.GetByIdAsync(request.Id, cancellationToken);
        if (variant is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ProductVariant", request.Id));
        }

        variant.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class IncreaseProductVariantStockCommandHandler(IRepository<ProductVariantEntity> variants, IUnitOfWork unitOfWork)
    : ICommandHandler<IncreaseProductVariantStockCommand>
{
    public async Task<Result> Handle(IncreaseProductVariantStockCommand request, CancellationToken cancellationToken)
    {
        var variant = await variants.GetByIdAsync(request.Id, cancellationToken);
        if (variant is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ProductVariant", request.Id));
        }

        variant.IncreaseStock(request.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DecreaseProductVariantStockCommandHandler(IRepository<ProductVariantEntity> variants, IUnitOfWork unitOfWork)
    : ICommandHandler<DecreaseProductVariantStockCommand>
{
    public async Task<Result> Handle(DecreaseProductVariantStockCommand request, CancellationToken cancellationToken)
    {
        var variant = await variants.GetByIdAsync(request.Id, cancellationToken);
        if (variant is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ProductVariant", request.Id));
        }

        variant.DecreaseStock(request.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetProductVariantByIdQueryHandler(
    IRepository<ProductVariantEntity> variants,
    IRepository<ProductEntity> products)
    : IQueryHandler<GetProductVariantByIdQuery, ProductVariantResponse>
{
    public Task<Result<ProductVariantResponse>> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
    {
        var variant = variants.Query().FirstOrDefault(variant =>
            variant.Id == request.Id &&
            variant.IsActive &&
            products.Query().Any(product => product.Id == variant.ProductId && product.IsActive));
        return Task.FromResult(variant is null
            ? Result.Failure<ProductVariantResponse>(ApplicationErrors.NotFound("ProductVariant", request.Id))
            : Result.Success(CatalogMappings.ToResponse(variant)));
    }
}

internal sealed class GetProductVariantsByProductQueryHandler(
    IRepository<ProductVariantEntity> variants,
    IRepository<ProductEntity> products)
    : IQueryHandler<GetProductVariantsByProductQuery, List<ProductVariantResponse>>
{
    public Task<Result<List<ProductVariantResponse>>> Handle(GetProductVariantsByProductQuery request, CancellationToken cancellationToken)
    {
        var query = variants.Query()
            .Where(variant =>
                variant.ProductId == request.ProductId &&
                (!request.OnlyActive || variant.IsActive) &&
                products.Query().Any(product => product.Id == variant.ProductId && product.IsActive));

        return Task.FromResult(Result.Success(query.Select(CatalogMappings.ToResponse).ToList()));
    }
}

internal sealed class CreateProductImageCommandHandler(
    IRepository<ProductEntity> products,
    IRepository<ProductVariantEntity> variants,
    IRepository<ProductImageEntity> images,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductImageCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductImageCommand request, CancellationToken cancellationToken)
    {
        var validation = CatalogMappings.ValidateProductImageTarget(request.ProductId, request.ProductVariantId, products, variants);
        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Error);
        }

        var image = ProductImageEntity.Create(request.ProductId, request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain);
        images.Add(image);
        CatalogMappings.NormalizeProductImages(request.ProductId, images, image.Id, request.IsMain, image);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return image.Id;
    }
}

internal sealed class UpdateProductImageCommandHandler(
    IRepository<ProductVariantEntity> variants,
    IRepository<ProductImageEntity> images,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductImageCommand>
{
    public async Task<Result> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await images.GetByIdAsync(request.Id, cancellationToken);
        if (image is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ProductImage", request.Id));
        }

        if (request.ProductVariantId.HasValue &&
            !variants.Query().Any(variant =>
                variant.Id == request.ProductVariantId.Value &&
                variant.ProductId == image.ProductId &&
                variant.IsActive))
        {
            return Result.Failure(ApplicationErrors.Conflict("ProductImage.InvalidVariant", "Product variant does not belong to the image product or is inactive."));
        }

        image.Update(request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain);
        CatalogMappings.NormalizeProductImages(image.ProductId, images, image.Id, request.IsMain);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ReorderProductImagesCommandHandler(
    IRepository<ProductImageEntity> images,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderProductImagesCommand>
{
    public async Task<Result> Handle(ReorderProductImagesCommand request, CancellationToken cancellationToken)
    {
        var productImages = images.Query()
            .Where(image => image.ProductId == request.ProductId)
            .ToList();
        if (productImages.Count == 0)
        {
            return Result.Failure(ApplicationErrors.NotFound("ProductImages", request.ProductId));
        }

        var requestedIds = request.ImageIds.Distinct().ToList();
        var existingIds = productImages.Select(image => image.Id).ToHashSet();
        if (requestedIds.Count != productImages.Count || !requestedIds.All(existingIds.Contains))
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "ProductImage.InvalidOrder",
                "The image order must contain every image of the product exactly once."));
        }

        var imageById = productImages.ToDictionary(image => image.Id);
        for (var index = 0; index < requestedIds.Count; index++)
        {
            var image = imageById[requestedIds[index]];
            image.Update(image.ProductVariantId, image.Url, image.AltText, index + 1, image.IsMain);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteProductImageCommandHandler(IRepository<ProductImageEntity> images, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductImageCommand>
{
    public async Task<Result> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await images.GetByIdAsync(request.Id, cancellationToken);
        if (image is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("ProductImage", request.Id));
        }

        var productId = image.ProductId;
        images.Remove(image);
        CatalogMappings.NormalizeProductImages(productId, images, null, false, null, request.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetProductImagesByProductQueryHandler(
    IRepository<ProductImageEntity> images,
    IRepository<ProductEntity> products)
    : IQueryHandler<GetProductImagesByProductQuery, List<ProductImageResponse>>
{
    public Task<Result<List<ProductImageResponse>>> Handle(GetProductImagesByProductQuery request, CancellationToken cancellationToken)
    {
        var productExists = products.Query().Any(product => product.Id == request.ProductId && product.IsActive);
        if (!productExists)
        {
            return Task.FromResult(Result.Failure<List<ProductImageResponse>>(
                ApplicationErrors.NotFound("Product", request.ProductId)));
        }

        var result = images.Query()
            .Where(image => image.ProductId == request.ProductId)
            .OrderBy(image => image.Order)
            .Select(image => new ProductImageResponse(image.Id, image.ProductId, image.ProductVariantId, image.Url, image.AltText, image.Order, image.IsMain))
            .ToList();

        return Task.FromResult(Result.Success(result));
    }
}

internal sealed class GetProductImageByIdQueryHandler(IRepository<ProductImageEntity> images)
    : IQueryHandler<GetProductImageByIdQuery, ProductImageResponse>
{
    public Task<Result<ProductImageResponse>> Handle(GetProductImageByIdQuery request, CancellationToken cancellationToken)
    {
        var image = images.Query().FirstOrDefault(item => item.Id == request.Id);
        return Task.FromResult(image is null
            ? Result.Failure<ProductImageResponse>(ApplicationErrors.NotFound("ProductImage", request.Id))
            : Result.Success(new ProductImageResponse(
                image.Id,
                image.ProductId,
                image.ProductVariantId,
                image.Url,
                image.AltText,
                image.Order,
                image.IsMain)));
    }
}

internal static class CatalogMappings
{
    public static void NormalizeProductImages(
        Guid productId,
        IRepository<ProductImageEntity> images,
        Guid? preferredMainImageId,
        bool forcePreferredMain,
        ProductImageEntity? pendingImage = null,
        Guid? excludedImageId = null)
    {
        var productImages = images.Query()
            .Where(image => image.ProductId == productId && (!excludedImageId.HasValue || image.Id != excludedImageId.Value))
            .OrderBy(image => image.Order)
            .ThenBy(image => image.CreatedAt)
            .ThenBy(image => image.Id)
            .ToList();

        if (pendingImage is not null && productImages.All(image => image.Id != pendingImage.Id))
        {
            productImages.Add(pendingImage);
            productImages = productImages
                .OrderBy(image => image.Order)
                .ThenBy(image => image.CreatedAt)
                .ThenBy(image => image.Id)
                .ToList();
        }

        var mainImageId = forcePreferredMain
            ? preferredMainImageId
            : productImages.FirstOrDefault(image => image.IsMain)?.Id;

        mainImageId ??= productImages.FirstOrDefault()?.Id;

        for (var index = 0; index < productImages.Count; index++)
        {
            var image = productImages[index];
            image.Update(
                image.ProductVariantId,
                image.Url,
                image.AltText,
                index + 1,
                image.Id == mainImageId);
        }
    }

    public static Result ValidateProductCatalog(
        Guid categoryId,
        Guid? collectionId,
        IRepository<CategoryEntity> categories,
        IRepository<CollectionEntity> collections)
    {
        if (!categories.Query().Any(category => category.Id == categoryId && category.IsActive))
        {
            return Result.Failure(ApplicationErrors.NotFound("Category", categoryId));
        }

        if (collectionId.HasValue &&
            !collections.Query().Any(collection => collection.Id == collectionId.Value && collection.IsActive))
        {
            return Result.Failure(ApplicationErrors.NotFound("Collection", collectionId.Value));
        }

        return Result.Success();
    }

    public static Result ValidateProductImageTarget(
        Guid productId,
        Guid? productVariantId,
        IRepository<ProductEntity> products,
        IRepository<ProductVariantEntity> variants)
    {
        if (!products.Query().Any(product => product.Id == productId && product.IsActive))
        {
            return Result.Failure(ApplicationErrors.NotFound("Product", productId));
        }

        if (productVariantId.HasValue &&
            !variants.Query().Any(variant =>
                variant.Id == productVariantId.Value &&
                variant.ProductId == productId &&
                variant.IsActive))
        {
            return Result.Failure(ApplicationErrors.Conflict("ProductImage.InvalidVariant", "Product variant does not belong to the selected product or is inactive."));
        }

        return Result.Success();
    }

    public static ProductResponse ToResponse(ProductEntity product)
    {
        return new ProductResponse(
            product.Id,
            product.CategoryId,
            product.CollectionId,
            product.Name,
            product.Description,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            product.IsActive);
    }

    public static ProductVariantResponse ToResponse(ProductVariantEntity variant)
    {
        return new ProductVariantResponse(
            variant.Id,
            variant.ProductId,
            variant.Size,
            variant.Color,
            variant.Sku.Value,
            variant.PhysicalStock,
            variant.Price?.Amount,
            variant.Price?.Currency,
            variant.ShippingWeightKg,
            variant.ShippingLengthCm,
            variant.ShippingWidthCm,
            variant.ShippingHeightCm,
            variant.IsActive);
    }
}
