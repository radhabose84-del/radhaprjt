using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnAutoComplete;

public sealed class GetPurchaseReturnAutoCompleteQueryHandler : IRequestHandler<GetPurchaseReturnAutoCompleteQuery, IReadOnlyList<PurchaseReturnListItemDto>>
{
    private readonly IPurchaseReturnQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetPurchaseReturnAutoCompleteQueryHandler(IPurchaseReturnQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<PurchaseReturnListItemDto>> Handle(GetPurchaseReturnAutoCompleteQuery request, CancellationToken ct)
    {
        var items = await _repo.AutocompleteAsync(request.Term, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "AutoComplete",
            actionCode: "GetPurchaseReturnAutoCompleteQuery",
            actionName: items.Count.ToString(),
            details: "Purchase Return autocomplete fetched.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return items;
    }
}
