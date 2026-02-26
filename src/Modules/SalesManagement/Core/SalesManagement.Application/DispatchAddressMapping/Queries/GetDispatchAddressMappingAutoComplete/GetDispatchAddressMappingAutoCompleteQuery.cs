using MediatR;
using SalesManagement.Application.DispatchAddressMapping.Dto;

namespace SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingAutoComplete
{
    public sealed record GetDispatchAddressMappingAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<DispatchAddressMappingLookupDto>>;
}
