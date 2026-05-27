using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.Domain.Events;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;

public sealed class UpdateReturnTypeCommandHandler : IRequestHandler<UpdateReturnTypeCommand, ReturnTypeDto>
{
    private readonly IReturnTypeCommandRepository _commandRepo;
    private readonly IReturnTypeQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateReturnTypeCommandHandler(
        IReturnTypeCommandRepository commandRepo,
        IReturnTypeQueryRepository queryRepo,
        IMapper mapper,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ReturnTypeDto> Handle(UpdateReturnTypeCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<DomainReturnType>(request);
        var updated = await _commandRepo.UpdateAsync(entity, ct);

        var fresh = await _queryRepo.GetByIdAsync(updated.Id, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "RETURNTYPE_UPDATE",
            actionName: request.Id.ToString(),
            details: $"ReturnType with Id {request.Id} updated.",
            module: "ReturnType");
        await _mediator.Publish(ev, ct);

        return fresh!;
    }
}
