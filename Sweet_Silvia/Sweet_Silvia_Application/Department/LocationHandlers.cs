using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.District;
using Sweet_Silvia_Application.Province;
using Sweet_Silvia_Domain.Abstractions;
using DepartmentEntity = Sweet_Silvia_Domain.Department.Department;
using DistrictEntity = Sweet_Silvia_Domain.District.District;
using ProvinceEntity = Sweet_Silvia_Domain.Province.Province;

namespace Sweet_Silvia_Application.Department;

internal sealed class CreateDepartmentCommandHandler(IRepository<DepartmentEntity> departments, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDepartmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = DepartmentEntity.Create(request.Name, request.DestinationType);
        departments.Add(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return department.Id;
    }
}

internal sealed class UpdateDepartmentCommandHandler(IRepository<DepartmentEntity> departments, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDepartmentCommand>
{
    public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await departments.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Department", request.Id));
        }

        department.Update(request.Name, request.DestinationType);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteDepartmentCommandHandler(IRepository<DepartmentEntity> departments, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteDepartmentCommand>
{
    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await departments.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Department", request.Id));
        }

        department.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetDepartmentByIdQueryHandler(IRepository<DepartmentEntity> departments)
    : IQueryHandler<GetDepartmentByIdQuery, DepartmentResponse>
{
    public Task<Result<DepartmentResponse>> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = departments.Query().FirstOrDefault(department => department.Id == request.Id && department.IsActive);
        return Task.FromResult(department is null
            ? Result.Failure<DepartmentResponse>(ApplicationErrors.NotFound("Department", request.Id))
            : Result.Success(new DepartmentResponse(department.Id, department.Name, department.DestinationType, department.IsActive)));
    }
}

internal sealed class GetDepartmentsQueryHandler(IRepository<DepartmentEntity> departments)
    : IQueryHandler<GetDepartmentsQuery, List<DepartmentResponse>>
{
    public Task<Result<List<DepartmentResponse>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = departments.Query().Where(department => department.IsActive);
        return Task.FromResult(Result.Success(query.Select(department => new DepartmentResponse(department.Id, department.Name, department.DestinationType, department.IsActive)).ToList()));
    }
}

internal sealed class CreateProvinceCommandHandler(
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProvinceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProvinceCommand request, CancellationToken cancellationToken)
    {
        if (!departments.Query().Any(department => department.Id == request.DepartmentId && department.IsActive))
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Department", request.DepartmentId));
        }

        var province = ProvinceEntity.Create(request.DepartmentId, request.Name);
        provinces.Add(province);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return province.Id;
    }
}

internal sealed class UpdateProvinceCommandHandler(
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProvinceCommand>
{
    public async Task<Result> Handle(UpdateProvinceCommand request, CancellationToken cancellationToken)
    {
        var province = await provinces.GetByIdAsync(request.Id, cancellationToken);
        if (province is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Province", request.Id));
        }

        if (!departments.Query().Any(department => department.Id == request.DepartmentId && department.IsActive))
        {
            return Result.Failure(ApplicationErrors.NotFound("Department", request.DepartmentId));
        }

        province.Update(request.DepartmentId, request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteProvinceCommandHandler(IRepository<ProvinceEntity> provinces, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProvinceCommand>
{
    public async Task<Result> Handle(DeleteProvinceCommand request, CancellationToken cancellationToken)
    {
        var province = await provinces.GetByIdAsync(request.Id, cancellationToken);
        if (province is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Province", request.Id));
        }

        province.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetProvinceByIdQueryHandler(
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments)
    : IQueryHandler<GetProvinceByIdQuery, ProvinceResponse>
{
    public Task<Result<ProvinceResponse>> Handle(GetProvinceByIdQuery request, CancellationToken cancellationToken)
    {
        var province = provinces.Query().FirstOrDefault(province =>
            province.Id == request.Id &&
            province.IsActive &&
            departments.Query().Any(department => department.Id == province.DepartmentId && department.IsActive));
        return Task.FromResult(province is null
            ? Result.Failure<ProvinceResponse>(ApplicationErrors.NotFound("Province", request.Id))
            : Result.Success(new ProvinceResponse(province.Id, province.DepartmentId, province.Name, province.IsActive)));
    }
}

internal sealed class GetProvincesByDepartmentQueryHandler(
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments)
    : IQueryHandler<GetProvincesByDepartmentQuery, List<ProvinceResponse>>
{
    public Task<Result<List<ProvinceResponse>>> Handle(GetProvincesByDepartmentQuery request, CancellationToken cancellationToken)
    {
        var query = provinces.Query()
            .Where(province => province.DepartmentId == request.DepartmentId && province.IsActive &&
                departments.Query().Any(department => department.Id == province.DepartmentId && department.IsActive));

        return Task.FromResult(Result.Success(query.Select(province => new ProvinceResponse(province.Id, province.DepartmentId, province.Name, province.IsActive)).ToList()));
    }
}

internal sealed class CreateDistrictCommandHandler(
    IRepository<DistrictEntity> districts,
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDistrictCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDistrictCommand request, CancellationToken cancellationToken)
    {
        if (!provinces.Query().Any(province => province.Id == request.ProvinceId && province.IsActive &&
            departments.Query().Any(department => department.Id == province.DepartmentId && department.IsActive)))
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Province", request.ProvinceId));
        }

        var district = DistrictEntity.Create(request.ProvinceId, request.Name, request.Ubigeo);
        districts.Add(district);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return district.Id;
    }
}

internal sealed class UpdateDistrictCommandHandler(
    IRepository<DistrictEntity> districts,
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDistrictCommand>
{
    public async Task<Result> Handle(UpdateDistrictCommand request, CancellationToken cancellationToken)
    {
        var district = await districts.GetByIdAsync(request.Id, cancellationToken);
        if (district is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("District", request.Id));
        }

        if (!provinces.Query().Any(province => province.Id == request.ProvinceId && province.IsActive &&
            departments.Query().Any(department => department.Id == province.DepartmentId && department.IsActive)))
        {
            return Result.Failure(ApplicationErrors.NotFound("Province", request.ProvinceId));
        }

        district.Update(request.ProvinceId, request.Name, request.Ubigeo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteDistrictCommandHandler(IRepository<DistrictEntity> districts, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteDistrictCommand>
{
    public async Task<Result> Handle(DeleteDistrictCommand request, CancellationToken cancellationToken)
    {
        var district = await districts.GetByIdAsync(request.Id, cancellationToken);
        if (district is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("District", request.Id));
        }

        district.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetDistrictByIdQueryHandler(
    IRepository<DistrictEntity> districts,
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments)
    : IQueryHandler<GetDistrictByIdQuery, DistrictResponse>
{
    public Task<Result<DistrictResponse>> Handle(GetDistrictByIdQuery request, CancellationToken cancellationToken)
    {
        var district = districts.Query().FirstOrDefault(district =>
            district.Id == request.Id &&
            district.IsActive &&
            provinces.Query().Any(province => province.Id == district.ProvinceId && province.IsActive &&
                departments.Query().Any(department => department.Id == province.DepartmentId && department.IsActive)));
        return Task.FromResult(district is null
            ? Result.Failure<DistrictResponse>(ApplicationErrors.NotFound("District", request.Id))
            : Result.Success(new DistrictResponse(district.Id, district.ProvinceId, district.Name, district.Ubigeo, district.IsActive)));
    }
}

internal sealed class GetDistrictsByProvinceQueryHandler(
    IRepository<DistrictEntity> districts,
    IRepository<ProvinceEntity> provinces,
    IRepository<DepartmentEntity> departments)
    : IQueryHandler<GetDistrictsByProvinceQuery, List<DistrictResponse>>
{
    public Task<Result<List<DistrictResponse>>> Handle(GetDistrictsByProvinceQuery request, CancellationToken cancellationToken)
    {
        var query = districts.Query()
            .Where(district => district.ProvinceId == request.ProvinceId && district.IsActive &&
                provinces.Query().Any(province => province.Id == district.ProvinceId && province.IsActive &&
                    departments.Query().Any(department => department.Id == province.DepartmentId && department.IsActive)));

        return Task.FromResult(Result.Success(query.Select(district => new DistrictResponse(district.Id, district.ProvinceId, district.Name, district.Ubigeo, district.IsActive)).ToList()));
    }
}
