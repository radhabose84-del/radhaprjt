using MediatR;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOAutoComplete
{
    public sealed record GetRawMaterialPOAutoCompleteQuery(string Term) : IRequest<IReadOnlyList<RawMaterialPOLookupDto>>;
}
