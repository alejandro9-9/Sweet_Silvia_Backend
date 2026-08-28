using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Collection;

public sealed record CollectionResponse(Guid Id, string Name, string Description, bool IsActive);

public sealed record CreateCollectionCommand(string Name, string Description) : ICommand<Guid>;

public sealed record UpdateCollectionCommand(Guid Id, string Name, string Description) : ICommand;

public sealed record DeleteCollectionCommand(Guid Id) : ICommand;

public sealed record GetCollectionByIdQuery(Guid Id) : IQuery<CollectionResponse>;

public sealed record GetCollectionsQuery(bool OnlyActive = true) : IQuery<List<CollectionResponse>>;
