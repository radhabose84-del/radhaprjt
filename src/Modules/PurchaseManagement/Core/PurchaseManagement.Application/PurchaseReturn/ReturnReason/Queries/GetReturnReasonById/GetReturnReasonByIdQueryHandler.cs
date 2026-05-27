using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonById;

public sealed class GetReturnReasonByIdQueryHandler : IRequestHandler<GetReturnReasonByIdQuery, ReturnReasonDto?>
{
    private readonly IReturnReasonQueryRepository _repo;
    private readonly IMediator _mediator;

    public GetReturnReasonByIdQueryHandler(IReturnReasonQueryRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<ReturnReasonDto?> Handle(GetReturnReasonByIdQuery request, CancellationToken ct)
    {
        var dto = await _repo.GetByIdAsync(request.Id, ct);
        if (dto == null)
            return null;

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetReturnReasonByIdQuery",
            actionName: dto.Id.ToString(),
            details: $"ReturnReason {dto.Id} fetched.",
            module: "ReturnReason");
        await _mediator.Publish(ev, ct);

        return dto;
    }
}
