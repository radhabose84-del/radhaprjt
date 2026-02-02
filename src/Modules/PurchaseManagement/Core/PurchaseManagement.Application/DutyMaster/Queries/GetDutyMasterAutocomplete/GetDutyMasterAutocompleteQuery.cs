using MediatR;

namespace PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterAutocomplete
{
    public sealed record GetDutyMasterAutocompleteQuery(string? Term, int Top = 10)
        : IRequest<IReadOnlyList<DutyMasterAutocompleteDto>>;
}
