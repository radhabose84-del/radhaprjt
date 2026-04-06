using MediatR;
using SalesManagement.Application.DiscountMaster.Dto;

namespace SalesManagement.Application.DiscountMaster.Queries.GetDiscountMasterAutoComplete
{
    public sealed record GetDiscountMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<DiscountMasterLookupDto>>;
}
