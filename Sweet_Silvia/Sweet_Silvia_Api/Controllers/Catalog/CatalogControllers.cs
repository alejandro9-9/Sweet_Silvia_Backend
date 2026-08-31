using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
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
public sealed class ProductImagesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductImagesByProductQuery(productId), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductImageRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateProductImageCommand(request.ProductId, request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductImageRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductImageCommand(id, request.ProductVariantId, request.Url, request.AltText, request.Order, request.IsMain), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteProductImageCommand(id), cancellationToken));
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
