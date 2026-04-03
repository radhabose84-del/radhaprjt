using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.PackType.Queries.GetPackTypeAutoComplete;

public sealed record GetPackTypeAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<PackTypeLookupDto>>;
