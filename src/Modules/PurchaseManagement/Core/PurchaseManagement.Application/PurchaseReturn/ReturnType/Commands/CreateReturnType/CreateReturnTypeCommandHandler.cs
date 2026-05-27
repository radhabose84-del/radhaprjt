using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.Domain.Events;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;

public sealed class CreateReturnTypeCommandHandler : IRequestHandler<CreateReturnTypeCommand, ReturnTypeDto>
{
    private readonly IReturnTypeCommandRepository _commandRepo;
    private readonly IReturnTypeQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateReturnTypeCommandHandler(
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

    public async Task<ReturnTypeDto> Handle(CreateReturnTypeCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<DomainReturnType>(request);
        var created = await _commandRepo.CreateAsync(entity, ct);

        var fresh = await _queryRepo.GetByIdAsync(created.Id, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: "RETURNTYPE_CREATE",
            actionName: request.Code,
            details: $"ReturnType '{request.Code}' created with Id {created.Id}.",
            module: "ReturnType");
        await _mediator.Publish(ev, ct);

        return fresh!;
    }
}
