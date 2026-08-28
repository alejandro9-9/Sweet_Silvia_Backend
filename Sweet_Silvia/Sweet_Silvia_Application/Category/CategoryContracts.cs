using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Category;

public sealed record CategoryResponse(Guid Id, string Name, string Description, bool IsActive);

public sealed record CreateCategoryCommand(string Name, string Description) : ICommand<Guid>;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string Description) : ICommand;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryResponse>;

public sealed record GetCategoriesQuery(bool OnlyActive = true) : IQuery<List<CategoryResponse>>;
