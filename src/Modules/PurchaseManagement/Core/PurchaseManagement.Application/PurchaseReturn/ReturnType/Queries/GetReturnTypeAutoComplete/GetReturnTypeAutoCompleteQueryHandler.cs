using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeAutoComplete;

public sealed class GetReturnTypeAutoCompleteQueryHandler : IRequestHandler<GetReturnTypeAutoCompleteQuery, IReadOnlyList<ReturnTypeLookupDto>>
{
    private readonly IReturnTypeQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnTypeAutoCompleteQueryHandler(IReturnTypeQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<ReturnTypeLookupDto>> Handle(GetReturnTypeAutoCompleteQuery request, CancellationToken ct)
    {
        var items = await _repo.AutocompleteAsync(request.Term, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "AutoComplete",
            actionCode: "GetReturnTypeAutoCompleteQuery",
            actionName: items.Count.ToString(),
            details: "ReturnType autocomplete fetched.",
            module: "ReturnType");
        await _mediator.Publish(ev, ct);

        return items;
    }
}
