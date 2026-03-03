using MediatR;
using SalesManagement.Application.PackType.Dto;

namespace SalesManagement.Application.PackType.Queries.GetPackTypeAutoComplete;

public sealed record GetPackTypeAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<PackTypeLookupDto>>;
