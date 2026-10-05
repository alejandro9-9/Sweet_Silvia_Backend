using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.Abstractions.Storage;
using Sweet_Silvia_Application.Category;
using Sweet_Silvia_Application.Collection;
using Sweet_Silvia_Application.Product;
using Sweet_Silvia_Application.ProductImage;
using Sweet_Silvia_Application.ProductVariant;

namespace Sweet_Silvia_Api.Controllers.Catalog;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetCategoriesQuery(onlyActive || !User.IsInRole(ApiRoles.AdministratorOrAssistant)), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCategoryByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateCategoryCommand(request.Name, request.Description), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateCategoryCommand(id, request.Name, request.Description), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteCategoryCommand(id), cancellationToken));
}

[ApiController]
[Route("api/collections")]
public sealed class CollectionsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetCollectionsQuery(onlyActive || !User.IsInRole(ApiRoles.AdministratorOrAssistant)), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCollectionByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCollectionRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateCollectionCommand(request.Name, request.Description), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCollectionRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateCollectionCommand(id, request.Name, request.Description), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteCollectionCommand(id), cancellationToken));
}

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, [FromQuery] Guid? collectionId, [FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetProductsQuery(categoryId, collectionId, onlyActive || !User.IsInRole(ApiRoles.AdministratorOrAssistant)), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateProductCommand(request.CategoryId, request.CollectionId, request.Name, request.Description, request.BasePrice, request.Currency), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductCommand(id, request.CategoryId, request.CollectionId, request.Name, request.Description, request.BasePrice, request.Currency), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPut("order")]
    public async Task<IActionResult> Reorder(ReorderProductsRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ReorderProductsCommand(request.ProductIds), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost("{id:guid}/move-to-top")]
    public async Task<IActionResult> MoveToTop(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new MoveProductToTopCommand(id), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteProductCommand(id), cancellationToken));
}

[ApiController]
[Route("api/product-variants")]
public sealed class ProductVariantsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductVariantByIdQuery(id), cancellationToken));

    [AllowAnonymous]
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId, [FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetProductVariantsByProductQuery(productId, onlyActive || !User.IsInRole(ApiRoles.AdministratorOrAssistant)), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductVariantRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateProductVariantCommand(request.ProductId, request.Size, request.Color, request.Sku, request.PhysicalStock, request.Price, request.Currency, request.ShippingWeightKg, request.ShippingLengthCm, request.ShippingWidthCm, request.ShippingHeightCm), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductVariantRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductVariantCommand(id, request.Size, request.Color, request.Price, request.Currency, request.ShippingWeightKg, request.ShippingLengthCm, request.ShippingWidthCm, request.ShippingHeightCm), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteProductVariantCommand(id), cancellationToken));
}

[ApiController]
[Route("api/product-images")]
public sealed class ProductImagesController(ISender sender, IFileStorageService fileStorage) : ControllerBase
{
    private const long MaximumImageSizeBytes = 15 * 1024 * 1024;
    private const long MaximumImageRequestSizeBytes = MaximumImageSizeBytes + 64 * 1024;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    [AllowAnonymous]
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductImagesByProductQuery(productId), cancellationToken));

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetByProducts([FromQuery] Guid[] productIds, CancellationToken cancellationToken)
    {
        var distinctProductIds = productIds.Distinct().ToArray();
        if (distinctProductIds.Length is 0 or > 100)
        {
            return BadRequest(new { message = "Selecciona entre 1 y 100 productos." });
        }

        return this.ToActionResult(await sender.Send(new GetProductImagesByProductsQuery(distinctProductIds), cancellationToken));
    }

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductImageRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateProductImageCommand(request.ProductId, request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumImageRequestSizeBytes)]
    public async Task<IActionResult> Upload([FromForm] UploadProductImageFileRequest request, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFileAsync(request.File, AllowedImageExtensions, MaximumImageSizeBytes, cancellationToken);
        if (validationError is not null)
        {
            return BadRequest(new { error = validationError });
        }

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

        var product = await sender.Send(new GetProductByIdQuery(request.ProductId), cancellationToken);
        if (product.IsFailure)
        {
            return this.ToActionResult(product);
        }

        if (!product.Value.IsActive)
        {
            return BadRequest(new { error = "El producto está inactivo y no admite nuevas imágenes." });
        }

        if (request.ProductVariantId.HasValue)
        {
            var variant = await sender.Send(new GetProductVariantByIdQuery(request.ProductVariantId.Value), cancellationToken);
            if (variant.IsFailure)
            {
                return this.ToActionResult(variant);
            }

            if (variant.Value.ProductId != request.ProductId)
            {
                return BadRequest(new { error = "La variante seleccionada no pertenece a este producto." });
            }

            if (!variant.Value.IsActive)
            {
                return BadRequest(new { error = "La variante seleccionada está inactiva." });
            }
        }

        await using var fileStream = request.File.OpenReadStream();
        var storedFile = await fileStorage.SaveAsync(
            new FileStorageUpload(
                "products",
                request.File.FileName,
                GetContentType(extension),
                fileStream),
            cancellationToken);

        try
        {
            var result = await sender.Send(
                new CreateProductImageCommand(
                    request.ProductId,
                    request.ProductVariantId,
                    storedFile.Url,
                    request.AltText,
                    request.Order,
                    request.IsMain),
                cancellationToken);

            if (result.IsFailure)
            {
                await fileStorage.DeleteAsync(storedFile.Url, cancellationToken);
                return BadRequest(UserFacingErrorMessages.Localize(result.Error));
            }

            return Ok(new { id = result.Value, url = storedFile.Url, key = storedFile.Key });
        }
        catch
        {
            await fileStorage.DeleteAsync(storedFile.Url, cancellationToken);
            throw;
        }
    }

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductImageRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductImageCommand(id, request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("product/{productId:guid}/order")]
    public async Task<IActionResult> Reorder(Guid productId, ReorderProductImagesRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ReorderProductImagesCommand(productId, request.ImageIds), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var image = await sender.Send(new GetProductImageByIdQuery(id), cancellationToken);
        if (image.IsFailure)
        {
            return this.ToActionResult(image);
        }

        var result = await sender.Send(new DeleteProductImageCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        // Delete the physical file only after the database record was removed.
        await fileStorage.DeleteAsync(image.Value.Url, cancellationToken);
        return this.ToActionResult(result);
    }

    private static async Task<string?> ValidateFileAsync(IFormFile file, HashSet<string> allowedExtensions, long maximumSizeBytes, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return "Selecciona una imagen para continuar.";
        }

        if (file.Length > maximumSizeBytes)
        {
            return $"La imagen es demasiado grande. El tamaño máximo permitido es {maximumSizeBytes / 1024 / 1024} MB.";
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return "Formato no permitido. Elige una imagen JPG, PNG o WEBP.";
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(), cancellationToken);
        var validSignature = extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => read >= 12 && header[..4].SequenceEqual("RIFF"u8.ToArray()) && header[8..12].SequenceEqual("WEBP"u8.ToArray()),
            _ => false
        };

        return validSignature ? null : "El archivo no parece ser una imagen válida. Elige un JPG, PNG o WEBP.";
    }

    private static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}

public sealed record CreateCategoryRequest(string Name, string Description);
public sealed record UpdateCategoryRequest(string Name, string Description);
public sealed record CreateCollectionRequest(string Name, string Description);
public sealed record UpdateCollectionRequest(string Name, string Description);
public sealed record CreateProductRequest(Guid CategoryId, Guid? CollectionId, string Name, string? Description, decimal BasePrice, string Currency = "PEN");
public sealed record UpdateProductRequest(Guid CategoryId, Guid? CollectionId, string Name, string? Description, decimal BasePrice, string Currency = "PEN");
public sealed record ReorderProductsRequest(IReadOnlyList<Guid> ProductIds);
public sealed record CreateProductVariantRequest(Guid ProductId, string Size, string Color, string Sku, int PhysicalStock, decimal? Price = null, string Currency = "PEN", decimal ShippingWeightKg = 0.3m, decimal ShippingLengthCm = 30m, decimal ShippingWidthCm = 22m, decimal ShippingHeightCm = 4m);
public sealed record UpdateProductVariantRequest(string Size, string Color, decimal? Price = null, string Currency = "PEN", decimal? ShippingWeightKg = null, decimal? ShippingLengthCm = null, decimal? ShippingWidthCm = null, decimal? ShippingHeightCm = null);
public sealed record CreateProductImageRequest(Guid ProductId, Guid? ProductVariantId, string Url, string? AltText, int Order, bool IsMain);
public sealed record UpdateProductImageRequest(Guid? ProductVariantId, string Url, string? AltText, int Order, bool IsMain);
public sealed record ReorderProductImagesRequest(IReadOnlyList<Guid> ImageIds);

public sealed class UploadProductImageFileRequest
{
    public Guid ProductId { get; init; }
    public Guid? ProductVariantId { get; init; }
    public string? AltText { get; init; }
    public int Order { get; init; }
    public bool IsMain { get; init; }
    public IFormFile File { get; init; } = null!;
}
