using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Domain.Events;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.UpdateReturnReason;

public sealed class UpdateReturnReasonCommandHandler : IRequestHandler<UpdateReturnReasonCommand, ReturnReasonDto>
{
    private readonly IReturnReasonCommandRepository _commandRepo;
    private readonly IReturnReasonQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateReturnReasonCommandHandler(
        IReturnReasonCommandRepository commandRepo,
        IReturnReasonQueryRepository queryRepo,
        IMapper mapper,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ReturnReasonDto> Handle(UpdateReturnReasonCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<DomainReturnReason>(request);
        var updated = await _commandRepo.UpdateAsync(entity, ct);

        var fresh = await _queryRepo.GetByIdAsync(updated.Id, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "RETURNREASON_UPDATE",
            actionName: request.Id.ToString(),
            details: $"ReturnReason with Id {request.Id} updated.",
            module: "ReturnReason");
        await _mediator.Publish(ev, ct);

        return fresh!;
    }
}
