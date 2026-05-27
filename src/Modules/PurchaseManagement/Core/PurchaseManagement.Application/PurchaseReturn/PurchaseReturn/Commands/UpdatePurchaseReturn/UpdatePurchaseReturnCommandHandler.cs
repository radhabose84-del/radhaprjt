using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.UpdatePurchaseReturn;

public sealed class UpdatePurchaseReturnCommandHandler : IRequestHandler<UpdatePurchaseReturnCommand, PurchaseReturnHeaderDto>
{
    private readonly IPurchaseReturnCommandRepository _commandRepo;
    private readonly IPurchaseReturnQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdatePurchaseReturnCommandHandler(
        IPurchaseReturnCommandRepository commandRepo,
        IPurchaseReturnQueryRepository queryRepo,
        IMapper mapper,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<PurchaseReturnHeaderDto> Handle(UpdatePurchaseReturnCommand request, CancellationToken ct)
    {
        var currentStatus = await _queryRepo.GetCurrentStatusCodeAsync(request.Id);
        if (currentStatus == null)
            throw new ExceptionRules("Purchase Return not found.");
        if (!string.Equals(currentStatus, MiscEnumEntity.Draft, StringComparison.OrdinalIgnoreCase))
            throw new ExceptionRules("Only Draft Purchase Returns can be updated.");

        var entity = _mapper.Map<PurchaseReturnHeader>(request);
        var updated = await _commandRepo.UpdateAsync(entity, ct);

        var fresh = await _queryRepo.GetByIdAsync(updated.Id, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "PURCHASERETURN_UPDATE",
            actionName: request.Id.ToString(),
            details: $"Purchase Return with Id {request.Id} updated.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return fresh!;
    }
}
