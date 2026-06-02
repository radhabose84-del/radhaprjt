using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnablePosByVendor;

public sealed class GetReturnablePosByVendorQueryHandler
    : IRequestHandler<GetReturnablePosByVendorQuery, IReadOnlyList<PurchaseReturnPoLookupDto>>
{
    private readonly IPurchaseReturnQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnablePosByVendorQueryHandler(IPurchaseReturnQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<PurchaseReturnPoLookupDto>> Handle(GetReturnablePosByVendorQuery request, CancellationToken ct)
    {
        var items = await _repo.GetPosByVendorAsync(request.VendorId, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetReturnablePosByVendor",
            actionCode: "GetReturnablePosByVendorQuery",
            actionName: request.VendorId.ToString(),
            details: $"Returnable POs fetched for VendorId={request.VendorId}; count={items.Count}.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return items;
    }
}
