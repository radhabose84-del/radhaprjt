using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetAllReturnTypes;

public sealed class GetAllReturnTypesQueryHandler : IRequestHandler<GetAllReturnTypesQuery, PagedResult<ReturnTypeDto>>
{
    private readonly IReturnTypeQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetAllReturnTypesQueryHandler(IReturnTypeQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<PagedResult<ReturnTypeDto>> Handle(GetAllReturnTypesQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetAll",
            actionCode: "GetAllReturnTypesQuery",
            actionName: items.Count.ToString(),
            details: "ReturnType list fetched.",
            module: "ReturnType");
        await _mediator.Publish(ev, ct);

        return new PagedResult<ReturnTypeDto>
        {
            Items = items.ToList(),
            Total = total,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
