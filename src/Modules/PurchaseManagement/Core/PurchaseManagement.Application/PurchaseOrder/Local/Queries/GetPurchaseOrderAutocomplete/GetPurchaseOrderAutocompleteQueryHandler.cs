using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAutocomplete;

public class GetPurchaseOrderAutocompleteHandler
    : IRequestHandler<GetPurchaseOrderAutocompleteQuery, IEnumerable<AutocompleteDto>>
{
    private readonly IPurchaseOrderQueryRepository _repo;
    public GetPurchaseOrderAutocompleteHandler(IPurchaseOrderQueryRepository repo) => _repo = repo;

    public Task<IEnumerable<AutocompleteDto>> Handle(GetPurchaseOrderAutocompleteQuery r, CancellationToken ct)
        => _repo.GetAutocompleteAsync(r.Term, r.poMethodId, r.budgetGroupId, ct);        
}
