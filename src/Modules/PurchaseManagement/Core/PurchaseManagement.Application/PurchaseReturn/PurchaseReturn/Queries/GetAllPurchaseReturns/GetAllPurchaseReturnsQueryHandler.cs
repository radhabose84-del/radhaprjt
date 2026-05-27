using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetAllPurchaseReturns;

public sealed class GetAllPurchaseReturnsQueryHandler : IRequestHandler<GetAllPurchaseReturnsQuery, PagedResult<PurchaseReturnListItemDto>>
{
    private readonly IPurchaseReturnQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetAllPurchaseReturnsQueryHandler(IPurchaseReturnQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<PagedResult<PurchaseReturnListItemDto>> Handle(GetAllPurchaseReturnsQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetAll",
            actionCode: "GetAllPurchaseReturnsQuery",
            actionName: items.Count.ToString(),
            details: "Purchase Return list fetched.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return new PagedResult<PurchaseReturnListItemDto>
        {
            Items = items.ToList(),
            Total = total,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
