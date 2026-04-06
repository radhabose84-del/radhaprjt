using MediatR;
using SalesManagement.Application.FreightMaster.Dto;

namespace SalesManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete
{
    public sealed record GetFreightMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<FreightMasterLookupDto>>;
}
