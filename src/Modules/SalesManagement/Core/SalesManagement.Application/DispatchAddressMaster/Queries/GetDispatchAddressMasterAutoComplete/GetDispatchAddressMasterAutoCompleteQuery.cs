using MediatR;
using SalesManagement.Application.DispatchAddressMaster.Dto;

namespace SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterAutoComplete;

public sealed record GetDispatchAddressMasterAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<DispatchAddressMasterLookupDto>>;
