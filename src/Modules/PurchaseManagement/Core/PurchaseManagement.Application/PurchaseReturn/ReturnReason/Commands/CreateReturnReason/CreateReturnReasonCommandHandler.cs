using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Domain.Events;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;

public sealed class CreateReturnReasonCommandHandler : IRequestHandler<CreateReturnReasonCommand, ReturnReasonDto>
{
    private readonly IReturnReasonCommandRepository _commandRepo;
    private readonly IReturnReasonQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateReturnReasonCommandHandler(
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

    public async Task<ReturnReasonDto> Handle(CreateReturnReasonCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<DomainReturnReason>(request);
        var created = await _commandRepo.CreateAsync(entity, ct);

        var fresh = await _queryRepo.GetByIdAsync(created.Id, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: "RETURNREASON_CREATE",
            actionName: request.Code,
            details: $"ReturnReason '{request.Code}' created with Id {created.Id}.",
            module: "ReturnReason");
        await _mediator.Publish(ev, ct);

        return fresh!;
    }
}
