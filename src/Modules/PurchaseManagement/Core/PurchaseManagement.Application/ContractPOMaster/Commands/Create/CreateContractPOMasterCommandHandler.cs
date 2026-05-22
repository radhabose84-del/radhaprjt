using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.ContractPOMaster.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.ContractPOMaster.Commands.Create;

public sealed class CreateContractPOMasterCommandHandler
    : IRequestHandler<CreateContractPOMasterCommand, ContractPOHeaderDto>
{
    private readonly IContractPOMasterCommandRepository _commandRepo;
    private readonly IContractPOMasterQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ipAddressService;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IAppDataMiscMasterLookup _appDataMiscLookup;
    private readonly IMiscMasterQueryRepository _misc;

    public CreateContractPOMasterCommandHandler(
        IContractPOMasterCommandRepository commandRepo,
        IContractPOMasterQueryRepository queryRepo,
        IMapper mapper,
        IMediator mediator,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ipAddressService,
        IOutboxEventPublisher outboxEventPublisher,
        IAppDataMiscMasterLookup appDataMiscLookup,
        IMiscMasterQueryRepository misc)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mapper = mapper;
        _mediator = mediator;
        _documentSequenceLookup = documentSequenceLookup;
        _ipAddressService = ipAddressService;
        _outboxEventPublisher = outboxEventPublisher;
        _appDataMiscLookup = appDataMiscLookup;
        _misc = misc;
    }

    public async Task<ContractPOHeaderDto> Handle(
        CreateContractPOMasterCommand request, CancellationToken ct)
    {
        // Map header
        var header = _mapper.Map<ContractPOHeader>(request);

        // Auto-fetch Pending status from MiscMaster
        var pendingStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        header.StatusId = pendingStatus.Id;

        // UnitId from token, not payload
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");
        header.UnitId = unitId;

        // Map and calculate detail values
        foreach (var detailItem in request.Details)
        {
            var detail = _mapper.Map<ContractPODetail>(detailItem);
            detail.ContractValue = detail.ContractQuantity * detail.ContractRate;
            detail.UtilizedQuantity = 0;
            detail.BalanceQuantity = detail.ContractQuantity;
            detail.UtilizedValue = 0;
            detail.BalanceValue = detail.ContractValue;
            header.ContractPODetails.Add(detail);
        }

        // Calculate header totals
        header.TotalContractValue = header.ContractPODetails.Sum(d => d.ContractValue);
        header.UtilizedValue = 0;
        header.BalanceValue = header.TotalContractValue;

        // Generate ContractPONumber from DocumentSequence
        var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
            MiscEnumEntity.TransactionTypeContract, MiscEnumEntity.ModulePurchase, unitId)
            ?? throw new InvalidOperationException("No transaction type configured for Contract PO.");
        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        header.ContractPONumber = sequences.Count > 0
            ? sequences[^1]
            : throw new InvalidOperationException("No document sequence configured for Contract PO.");

        // Persist (IncrementDocNoAsync called inside repo transaction)
        var created = await _commandRepo.CreateAsync(header, transactionTypeId, ct);

        // Fetch fresh record with lookups
        var dto = await _queryRepo.GetByIdAsync(created.Id, ct)
                  ?? throw new ExceptionRules("Failed to read newly created Contract PO.");

        // ── Approval / Notification (outbox) ─────────────────────────────────────
        var correlationId = Guid.NewGuid();

        var reversePayload = new CreateContractPOMasterReverseDto
        {
            Header = new ContractPOMasterWorkFlowDto
            {
                Id = created.Id,
                ContractPONumber = dto.ContractPONumber,
                VendorId = dto.VendorId,
                StatusId = dto.StatusId,
                UnitId = unitId
            },
            Lines = request.Details.Select(_ => new ContractPOMasterWorkFlowDto
            {
                Id = created.Id,
                ContractPONumber = dto.ContractPONumber,
                VendorId = dto.VendorId,
                StatusId = dto.StatusId,
                UnitId = unitId
            }).ToList()
        };
        var serializedPayload = JsonSerializer.Serialize(reversePayload);

        var workflowCommand = new CreateApprovalRequestCommand
        {
            CorrelationId = correlationId,
            ModuleTypeName = MiscEnumEntity.TransactionTypeContract,
            ModuleTransactionId = created.Id,
            Payload = serializedPayload,
            TransactionTypeId = transactionTypeId
        };

        var notificationEventMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(
            NotificationEnum.NotificationEvent, NotificationEnum.Create);

        var createdByName = _ipAddressService.GetUserName() ?? string.Empty;

        var notificationEvent = new NotificationCreatedEvent
        {
            CorrelationId = correlationId,
            CreatedByName = createdByName,
            UnitId = unitId,
            ModuleName = "Purchase Contract",
            EventTypeId = notificationEventMisc?.Id ?? 0,
            param1 = dto.ContractPONumber ?? string.Empty,
            param2 = createdByName,
            param3 = request.ContractDate,
            param4 = header.TotalContractValue.ToString(),
            param5 = dto.VendorName ?? string.Empty,
            ModuleTransactionId = created.Id,
            ModuleTypeName = MiscEnumEntity.TransactionTypeContract
        };

        // CreateAsync manages its own transaction, so use ScheduleAsync (separate save)
        await _outboxEventPublisher.ScheduleAsync(workflowCommand, correlationId, ct);
        //await _outboxEventPublisher.ScheduleAsync(notificationEvent, correlationId, ct);

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: dto.ContractPONumber ?? created.Id.ToString(),
            actionName: $"Contract PO for Vendor {dto.VendorId}",
            details: $"Contract PO '{dto.ContractPONumber}' created successfully with Id {created.Id}.",
            module: "ContractPO"
        );
        await _mediator.Publish(ev, ct);

        return dto;
    }
}
