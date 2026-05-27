using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeById;

public sealed class GetReturnTypeByIdQueryHandler : IRequestHandler<GetReturnTypeByIdQuery, ReturnTypeDto?>
{
    private readonly IReturnTypeQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnTypeByIdQueryHandler(IReturnTypeQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<ReturnTypeDto?> Handle(GetReturnTypeByIdQuery request, CancellationToken ct)
    {
        var dto = await _repo.GetByIdAsync(request.Id, ct);
        if (dto == null)
            return null;

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetReturnTypeByIdQuery",
            actionName: dto.Id.ToString(),
            details: $"ReturnType {dto.Id} fetched.",
            module: "ReturnType");
        await _mediator.Publish(ev, ct);

        return dto;
    }
}
