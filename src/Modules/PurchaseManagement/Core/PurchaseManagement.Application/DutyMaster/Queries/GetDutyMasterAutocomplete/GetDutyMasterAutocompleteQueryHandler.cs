using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using MediatR;

namespace PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterAutocomplete
{
    public class GetDutyMasterAutocompleteQueryHandler(
        IDutyMasterQueryRepository read
    ) : IRequestHandler<GetDutyMasterAutocompleteQuery, IReadOnlyList<DutyMasterAutocompleteDto>>
    {
        public Task<IReadOnlyList<DutyMasterAutocompleteDto>> Handle(GetDutyMasterAutocompleteQuery r, CancellationToken ct)
            => read.GetAutocompleteAsync(r.Term, ct);
    }
}
