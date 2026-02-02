using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using MediatR;


namespace PurchaseManagement.Application.Port.Queries.GetPortAutocomplete;
public sealed record GetPortAutocompleteQuery(string Term)
    : IRequest<IReadOnlyList<PortLookupDto>>;
