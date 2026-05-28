using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableQtyByGrn;

public sealed class GetReturnableQtyByGrnQueryHandler : IRequestHandler<GetReturnableQtyByGrnQuery, IReadOnlyList<ReturnableQtyDto>>
{
    private readonly IPurchaseReturnQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnableQtyByGrnQueryHandler(IPurchaseReturnQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<ReturnableQtyDto>> Handle(GetReturnableQtyByGrnQuery request, CancellationToken ct)
    {
        var items = await _repo.GetReturnableQtyByGrnAsync(request.GrnHeaderId, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetReturnableQty",
            actionCode: "GetReturnableQtyByGrnQuery",
            actionName: request.GrnHeaderId.ToString(),
            details: $"Returnable qty fetched for GrnHeaderId={request.GrnHeaderId}; lines={items.Count}.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return items;
    }
}
