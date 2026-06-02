using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.SubmitPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CreatePurchaseReturn;

public sealed class CreatePurchaseReturnCommandHandler : IRequestHandler<CreatePurchaseReturnCommand, PurchaseReturnHeaderDto>
{
    private readonly IPurchaseReturnCommandRepository _commandRepo;
    private readonly IPurchaseReturnQueryRepository _queryRepo;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ip;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreatePurchaseReturnCommandHandler(
        IPurchaseReturnCommandRepository commandRepo,
        IPurchaseReturnQueryRepository queryRepo,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ip,
        IMapper mapper,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _documentSequenceLookup = documentSequenceLookup;
        _ip = ip;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<PurchaseReturnHeaderDto> Handle(CreatePurchaseReturnCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<PurchaseReturnHeader>(request);

        // Resolve initial status — shared ApprovalStatus 'Pending' (no Draft state; posts straight to approval)
        var pendingStatusId = await _queryRepo.GetStatusIdByCodeAsync(MiscEnumEntity.Pending);
        if (!pendingStatusId.HasValue)
            throw new ExceptionRules("Approval status 'Pending' not found in MiscMaster.");
        entity.StatusId = pendingStatusId.Value;

        // Generate RtvNumber via DocumentSequence (Purchase module)
        var unitId = _ip.GetUnitId() ?? request.UnitId;
        var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
            MiscEnumEntity.TransactionTypePurchaseReturn, MiscEnumEntity.ModulePurchase, unitId);
        if (!typeId.HasValue)
            throw new ExceptionRules("Transaction Type 'Purchase Return' not configured for Purchase module.");

        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
        entity.RtvNumber = sequences.Count > 0 ? sequences[^1]
            : throw new ExceptionRules("No document sequence configured for Purchase Return.");

        var created = await _commandRepo.CreateAsync(entity, typeId.Value, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: "PURCHASERETURN_CREATE",
            actionName: entity.RtvNumber,
            details: $"Purchase Return '{entity.RtvNumber}' created with Id {created.Id}.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        // Post straight to approval — Purchase Returns have no resting Draft state.
        // Submit transitions Draft → PendingApproval and publishes the approval request,
        // so the RTV lands on the approval screen immediately on create.
        await _mediator.Send(new SubmitPurchaseReturnCommand(created.Id), ct);

        // Re-fetch after submit so the returned DTO reflects PendingApproval.
        var fresh = await _queryRepo.GetByIdAsync(created.Id, ct);
        return fresh!;
    }
}
