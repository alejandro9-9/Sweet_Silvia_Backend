using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Department;
using Sweet_Silvia_Application.District;
using Sweet_Silvia_Application.Province;
using Sweet_Silvia_Domain.Shared.Enums;

namespace Sweet_Silvia_Api.Controllers.Ubigeo;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetDepartmentsQuery(onlyActive || !User.IsInRole(ApiRoles.AdministratorOrAssistant)), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetDepartmentByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateDepartmentCommand(request.Name, request.DestinationType), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateDepartmentCommand(id, request.Name, request.DestinationType), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteDepartmentCommand(id), cancellationToken));
}

[ApiController]
[Route("api/provinces")]
public sealed class ProvincesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("department/{departmentId:guid}")]
    public async Task<IActionResult> GetByDepartment(Guid departmentId, [FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetProvincesByDepartmentQuery(departmentId, onlyActive || !User.IsInRole(ApiRoles.AdministratorOrAssistant)), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProvinceByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProvinceRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateProvinceCommand(request.DepartmentId, request.Name), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProvinceRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProvinceCommand(id, request.DepartmentId, request.Name), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteProvinceCommand(id), cancellationToken));
}

[ApiController]
[Route("api/districts")]
public sealed class DistrictsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetAllDistrictsQuery(onlyActive), cancellationToken));

    [AllowAnonymous]
    [HttpGet("province/{provinceId:guid}")]
    public async Task<IActionResult> GetByProvince(Guid provinceId, [FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetDistrictsByProvinceQuery(provinceId, onlyActive || !User.IsInRole(ApiRoles.AdministratorOrAssistant)), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetDistrictByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDistrictRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateDistrictCommand(request.ProvinceId, request.Name, request.Ubigeo), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateDistrictRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateDistrictCommand(id, request.ProvinceId, request.Name, request.Ubigeo), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteDistrictCommand(id), cancellationToken));
}

public sealed record CreateDepartmentRequest(string Name, ShippingDestinationType DestinationType);
public sealed record UpdateDepartmentRequest(string Name, ShippingDestinationType DestinationType);
public sealed record CreateProvinceRequest(Guid DepartmentId, string Name);
public sealed record UpdateProvinceRequest(Guid DepartmentId, string Name);
public sealed record CreateDistrictRequest(Guid ProvinceId, string Name, string? Ubigeo);
public sealed record UpdateDistrictRequest(Guid ProvinceId, string Name, string? Ubigeo);
