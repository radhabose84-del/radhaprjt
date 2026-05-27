using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnById;

public sealed class GetPurchaseReturnByIdQueryHandler : IRequestHandler<GetPurchaseReturnByIdQuery, PurchaseReturnHeaderDto?>
{
    private readonly IPurchaseReturnQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetPurchaseReturnByIdQueryHandler(IPurchaseReturnQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<PurchaseReturnHeaderDto?> Handle(GetPurchaseReturnByIdQuery request, CancellationToken ct)
    {
        var dto = await _repo.GetByIdAsync(request.Id, ct);
        if (dto == null)
            return null;

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetPurchaseReturnByIdQuery",
            actionName: dto.Id.ToString(),
            details: $"Purchase Return {dto.Id} fetched.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return dto;
    }
}
