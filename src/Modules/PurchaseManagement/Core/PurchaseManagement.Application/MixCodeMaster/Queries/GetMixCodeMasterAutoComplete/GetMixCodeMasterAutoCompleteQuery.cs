using MediatR;
using PurchaseManagement.Application.MixCodeMaster.Dto;

namespace PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterAutoComplete
{
    public sealed record GetMixCodeMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<MixCodeMasterLookupDto>>;
}
