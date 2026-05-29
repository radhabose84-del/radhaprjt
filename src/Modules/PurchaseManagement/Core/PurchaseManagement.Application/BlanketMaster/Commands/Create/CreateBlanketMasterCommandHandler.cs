using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BlanketMaster.Commands.Create;

public class CreateBlanketMasterCommandHandler : IRequestHandler<CreateBlanketMasterCommand, BlanketHeaderDto>
{
    private readonly IBlanketMasterCommandRepository _commandRepo;
    private readonly IBlanketMasterQueryRepository _queryRepo;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IIPAddressService _ipAddressService;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IMiscMasterQueryRepository _misc;

    public CreateBlanketMasterCommandHandler(
        IBlanketMasterCommandRepository commandRepo,
        IBlanketMasterQueryRepository queryRepo,
        IMediator mediator,
        IMapper mapper,
        IIPAddressService ipAddressService,
        IDocumentSequenceLookup documentSequenceLookup,
        IOutboxEventPublisher outboxEventPublisher,
        IMiscMasterQueryRepository misc)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
        _mapper = mapper;
        _ipAddressService = ipAddressService;
        _documentSequenceLookup = documentSequenceLookup;
        _outboxEventPublisher = outboxEventPublisher;
        _misc = misc;
    }

    public async Task<BlanketHeaderDto> Handle(CreateBlanketMasterCommand request, CancellationToken cancellationToken)
    {
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new InvalidOperationException("UnitId is not available for the current user.");

        // Resolve Pending status from MiscMaster
        var pendingStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        var pendingStatusId = pendingStatus.Id;

        // Map command → domain entity
        var header = _mapper.Map<BlanketHeader>(request);
        header.UnitId = unitId;
        header.StatusId = pendingStatusId;

        // Build details with calculated values
        var details = new List<BlanketDetail>();
        foreach (var item in request.Details)
        {
            var detail = new BlanketDetail
            {
                ItemSno = item.ItemSno,
                ItemId = item.ItemId,
                UOMId = item.UOMId,
                EstimatedQuantity = item.EstimatedQuantity,
                Rate = item.Rate,
                TotalPrice = item.EstimatedQuantity * item.Rate,
                HSNId = item.HSNId,
                GSTPercentage = item.GSTPercentage,
                QualitySpecification = item.QualitySpecification,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

            // Build schedules
            foreach (var sched in item.Schedules)
            {
                detail.Schedules.Add(new BlanketSchedule
                {
                    ScheduleNo = sched.ScheduleNo,
                    ScheduleDate = sched.ScheduleDate,
                    ScheduleQuantity = sched.ScheduleQuantity,
                    Remarks = sched.Remarks,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            }

            details.Add(detail);
        }

        header.Details = details;
        header.TotalEstimatedValue = details.Sum(d => d.TotalPrice);

        // Get transaction type for document number generation
        var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
            MiscEnumEntity.TransactionTypeBlanket,
            MiscEnumEntity.ModulePurchase,
            unitId)
            ?? throw new InvalidOperationException("No transaction type configured for Blanket PO.");

        // Create with auto-generated BlanketNumber
        var created = await _commandRepo.CreateAsync(header, transactionTypeId, cancellationToken);

        // Fetch fresh record with lookups
        var result = await _queryRepo.GetByIdAsync(created.Id, cancellationToken);

        // Workflow — schedule approval request
        var workflowEntity = await _commandRepo.GetByIdBlanketWorkFlowAsync(created.Id);
        if (workflowEntity != null)
        {
            var reversePayload = new CreateBlanketMasterReverseDto
            {
                Header = workflowEntity,
                Lines = null
            };
            var serializedPayload = System.Text.Json.JsonSerializer.Serialize(reversePayload);
            var correlationId = Guid.NewGuid();

            var workflowCommand = new Contracts.Commands.Workflow.CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TransactionTypeBlanket,
                ModuleTransactionId = created.Id,
                Payload = serializedPayload
                //,TransactionTypeId = transactionTypeId
            };

            await _outboxEventPublisher.ScheduleAsync(workflowCommand, correlationId, cancellationToken);
        }

        // Audit
        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: "BLANKET_MASTER_CREATE",
            actionName: created.BlanketNumber,
            details: $"Blanket Master '{created.BlanketNumber}' created successfully with Id {created.Id}.",
            module: "BlanketMaster"
        );
        await _mediator.Publish(auditEvent, cancellationToken);

        return result!;
    }
}
