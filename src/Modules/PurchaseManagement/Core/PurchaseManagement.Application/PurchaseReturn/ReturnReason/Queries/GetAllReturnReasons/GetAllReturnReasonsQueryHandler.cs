using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetAllReturnReasons;

public sealed class GetAllReturnReasonsQueryHandler : IRequestHandler<GetAllReturnReasonsQuery, PagedResult<ReturnReasonDto>>
{
    private readonly IReturnReasonQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetAllReturnReasonsQueryHandler(IReturnReasonQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<PagedResult<ReturnReasonDto>> Handle(GetAllReturnReasonsQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetAll",
            actionCode: "GetAllReturnReasonsQuery",
            actionName: items.Count.ToString(),
            details: "ReturnReason list fetched.",
            module: "ReturnReason");
        await _mediator.Publish(ev, ct);

        return new PagedResult<ReturnReasonDto>
        {
            Items = items.ToList(),
            Total = total,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
