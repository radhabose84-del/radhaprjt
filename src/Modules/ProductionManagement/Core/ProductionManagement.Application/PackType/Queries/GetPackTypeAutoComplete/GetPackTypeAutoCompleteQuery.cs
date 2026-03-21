using MediatR;
using ProductionManagement.Application.PackType.Dto;

namespace ProductionManagement.Application.PackType.Queries.GetPackTypeAutoComplete;

public sealed record GetPackTypeAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<PackTypeLookupDto>>;
