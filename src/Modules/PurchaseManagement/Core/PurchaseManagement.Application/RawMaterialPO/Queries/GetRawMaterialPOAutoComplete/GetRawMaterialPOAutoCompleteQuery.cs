using MediatR;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOAutoComplete
{
    public sealed record GetRawMaterialPOAutoCompleteQuery(string Term, bool ShowAll = false) : IRequest<IReadOnlyList<RawMaterialPOLookupDto>>;
}
