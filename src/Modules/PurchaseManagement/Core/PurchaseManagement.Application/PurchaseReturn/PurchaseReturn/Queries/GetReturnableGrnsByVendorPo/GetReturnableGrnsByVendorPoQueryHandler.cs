using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableGrnsByVendorPo;

public sealed class GetReturnableGrnsByVendorPoQueryHandler
    : IRequestHandler<GetReturnableGrnsByVendorPoQuery, IReadOnlyList<PurchaseReturnGrnLookupDto>>
{
    private readonly IPurchaseReturnQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnableGrnsByVendorPoQueryHandler(IPurchaseReturnQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<PurchaseReturnGrnLookupDto>> Handle(GetReturnableGrnsByVendorPoQuery request, CancellationToken ct)
    {
        var items = await _repo.GetGrnsByVendorPoAsync(request.VendorId, request.PoId, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetReturnableGrnsByVendorPo",
            actionCode: "GetReturnableGrnsByVendorPoQuery",
            actionName: $"{request.VendorId}/{request.PoId}",
            details: $"Returnable GRNs fetched for VendorId={request.VendorId}, PoId={request.PoId}; count={items.Count}.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return items;
    }
}
