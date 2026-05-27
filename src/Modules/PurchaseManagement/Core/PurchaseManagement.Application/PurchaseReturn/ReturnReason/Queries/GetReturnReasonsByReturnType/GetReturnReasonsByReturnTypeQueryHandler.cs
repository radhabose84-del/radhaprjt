using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonsByReturnType;

public sealed class GetReturnReasonsByReturnTypeQueryHandler
    : IRequestHandler<GetReturnReasonsByReturnTypeQuery, IReadOnlyList<ReturnReasonLookupDto>>
{
    private readonly IReturnReasonQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnReasonsByReturnTypeQueryHandler(IReturnReasonQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<ReturnReasonLookupDto>> Handle(GetReturnReasonsByReturnTypeQuery request, CancellationToken ct)
    {
        var items = await _repo.GetByReturnTypeIdAsync(request.ReturnTypeId, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetByReturnType",
            actionCode: "GetReturnReasonsByReturnTypeQuery",
            actionName: request.ReturnTypeId.ToString(),
            details: $"ReturnReasons fetched for ReturnTypeId={request.ReturnTypeId}, count={items.Count}.",
            module: "ReturnReason");
        await _mediator.Publish(ev, ct);

        return items;
    }
}
