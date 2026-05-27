using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonAutoComplete;

public sealed class GetReturnReasonAutoCompleteQueryHandler : IRequestHandler<GetReturnReasonAutoCompleteQuery, IReadOnlyList<ReturnReasonLookupDto>>
{
    private readonly IReturnReasonQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnReasonAutoCompleteQueryHandler(IReturnReasonQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<ReturnReasonLookupDto>> Handle(GetReturnReasonAutoCompleteQuery request, CancellationToken ct)
    {
        var items = await _repo.AutocompleteAsync(request.Term, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "AutoComplete",
            actionCode: "GetReturnReasonAutoCompleteQuery",
            actionName: items.Count.ToString(),
            details: "ReturnReason autocomplete fetched.",
            module: "ReturnReason");
        await _mediator.Publish(ev, ct);

        return items;
    }
}
