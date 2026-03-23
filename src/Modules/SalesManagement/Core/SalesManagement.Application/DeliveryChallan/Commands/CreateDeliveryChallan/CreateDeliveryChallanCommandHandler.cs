using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using System.Text.Json;

namespace SalesManagement.Application.DeliveryChallan.Commands.CreateDeliveryChallan
{
    public class CreateDeliveryChallanCommandHandler : IRequestHandler<CreateDeliveryChallanCommand, ApiResponseDTO<int>>
    {
        private readonly IDeliveryChallanCommandRepository _commandRepository;
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDeliveryChallanCommandHandler(
            IDeliveryChallanCommandRepository commandRepository,
            IDeliveryChallanQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDeliveryChallanCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<DeliveryChallanHeader>(request);

            // Set Pending status from ApprovalStatus
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StoApprovalStatus, MiscEnumEntity.StoApprovalPending);
            entity.StatusId = pendingStatus?.Id ?? 0;

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId();

            // Generate DC Number from Finance.DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeStodc, MiscEnumEntity.ModuleSales, unitId ?? 0);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'STO Delivery Challan' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var deliveryNumber = sequences.Count > 0 ? sequences[^1] : null;
            entity.DeliveryNumber = deliveryNumber
                ?? throw new ExceptionRules("No document sequence configured for STO Delivery Challan.");

            // Calculate DeliveryValue = SUM(LineMovementValues)
            if (entity.DeliveryChallanDetails != null && entity.DeliveryChallanDetails.Count > 0)
            {
                entity.DeliveryValue = entity.DeliveryChallanDetails.Sum(d => d.LineMovementValue);
            }

            // Default ConsignmentValue to DeliveryValue if not provided or zero
            if (request.ConsignmentValue <= 0)
            {
                entity.ConsignmentValue = entity.DeliveryValue;
            }

            // Resolve Reserved status ID — DC creation marks packs as Reserved (Packed → Reserved)
            var reservedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var reservedStatusId = reservedStatus?.Id ?? 0;

            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedStatusId = packedStatus?.Id ?? 0;

            var newId = await _commandRepository.CreateAsync(entity, packedStatusId, reservedStatusId, typeId.Value);

            // Publish workflow approval request via Outbox
            var correlationId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                Header = new
                {
                    Id = newId,
                    DeliveryNumber = deliveryNumber,
                    StoHeaderId = request.StoHeaderId,
                    UnitId = unitId ?? 0,
                    StatusId = entity.StatusId
                },
                Lines = new List<object>()
            });

            var workflowEvent = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.DCModuleTypeName,
                ModuleTransactionId = newId,
                Payload = payload
            };
            await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DELIVERYCHALLAN_CREATE",
                actionName: deliveryNumber,
                details: $"Delivery Challan '{deliveryNumber}' created successfully with Id {newId}.",
                module: "DeliveryChallan");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Delivery Challan created successfully.",
                Data = newId
            };
        }
    }
}
