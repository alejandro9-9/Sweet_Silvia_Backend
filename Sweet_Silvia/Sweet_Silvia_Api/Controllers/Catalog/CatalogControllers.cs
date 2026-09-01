using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
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
        => this.ToActionResult(await sender.Send(new GetCategoriesQuery(onlyActive), cancellationToken));

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
        => this.ToActionResult(await sender.Send(new GetCollectionsQuery(onlyActive), cancellationToken));

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
        => this.ToActionResult(await sender.Send(new GetProductsQuery(categoryId, collectionId, onlyActive), cancellationToken));

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
        => this.ToActionResult(await sender.Send(new GetProductVariantsByProductQuery(productId, onlyActive), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductVariantRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateProductVariantCommand(request.ProductId, request.Size, request.Color, request.Sku, request.PhysicalStock, request.Price, request.Currency), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductVariantRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductVariantCommand(id, request.Size, request.Color, request.Price, request.Currency), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteProductVariantCommand(id), cancellationToken));
}

[ApiController]
[Route("api/product-images")]
public sealed class ProductImagesController(ISender sender, IFileStorageService fileStorage) : ControllerBase
{
    private const long MaximumImageSizeBytes = 5 * 1024 * 1024;

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

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductImageRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateProductImageCommand(request.ProductId, request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumImageSizeBytes)]
    public async Task<IActionResult> Upload([FromForm] UploadProductImageFileRequest request, CancellationToken cancellationToken)
    {
        var validationError = ValidateFile(request.File, AllowedImageExtensions, MaximumImageSizeBytes);
        if (validationError is not null)
        {
            return BadRequest(new { error = validationError });
        }

        var product = await sender.Send(new GetProductByIdQuery(request.ProductId), cancellationToken);
        if (product.IsFailure)
        {
            return this.ToActionResult(product);
        }

        if (!product.Value.IsActive)
        {
            return BadRequest(new { error = "Product is inactive." });
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
                return BadRequest(new { error = "Product variant does not belong to the selected product." });
            }

            if (!variant.Value.IsActive)
            {
                return BadRequest(new { error = "Product variant is inactive." });
            }
        }

        var storedFile = await fileStorage.SaveAsync(
            new FileStorageUpload(
                "products",
                request.File.FileName,
                request.File.ContentType,
                request.File.OpenReadStream()),
            cancellationToken);

        var result = await sender.Send(
            new CreateProductImageCommand(
                request.ProductId,
                request.ProductVariantId,
                storedFile.Url,
                request.AltText,
                request.Order,
                request.IsMain),
            cancellationToken);

        return result.IsSuccess
            ? Ok(new { id = result.Value, url = storedFile.Url, key = storedFile.Key })
            : BadRequest(result.Error);
    }

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductImageRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductImageCommand(id, request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteProductImageCommand(id), cancellationToken));

    private static string? ValidateFile(IFormFile file, HashSet<string> allowedExtensions, long maximumSizeBytes)
    {
        if (file is null || file.Length == 0)
        {
            return "File is required.";
        }

        if (file.Length > maximumSizeBytes)
        {
            return $"File should not exceed {maximumSizeBytes / 1024 / 1024} MB.";
        }

        var extension = Path.GetExtension(file.FileName);
        return allowedExtensions.Contains(extension)
            ? null
            : $"File extension '{extension}' is not allowed.";
    }
}

public sealed record CreateCategoryRequest(string Name, string Description);
public sealed record UpdateCategoryRequest(string Name, string Description);
public sealed record CreateCollectionRequest(string Name, string Description);
public sealed record UpdateCollectionRequest(string Name, string Description);
public sealed record CreateProductRequest(Guid CategoryId, Guid? CollectionId, string Name, string? Description, decimal BasePrice, string Currency = "PEN");
public sealed record UpdateProductRequest(Guid CategoryId, Guid? CollectionId, string Name, string? Description, decimal BasePrice, string Currency = "PEN");
public sealed record CreateProductVariantRequest(Guid ProductId, string Size, string Color, string Sku, int PhysicalStock, decimal? Price = null, string Currency = "PEN");
public sealed record UpdateProductVariantRequest(string Size, string Color, decimal? Price = null, string Currency = "PEN");
public sealed record CreateProductImageRequest(Guid ProductId, Guid? ProductVariantId, string Url, string? AltText, int Order, bool IsMain);
public sealed record UpdateProductImageRequest(Guid? ProductVariantId, string Url, string? AltText, int Order, bool IsMain);

public sealed class UploadProductImageFileRequest
{
    public Guid ProductId { get; init; }
    public Guid? ProductVariantId { get; init; }
    public string? AltText { get; init; }
    public int Order { get; init; }
    public bool IsMain { get; init; }
    public IFormFile File { get; init; } = null!;
}
