using AutoMapper;
using Contracts.Common;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Domain.Events;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq
{
    public class CreateFreightRfqCommandHandler : IRequestHandler<CreateFreightRfqCommand, ApiResponseDTO<int>>
    {
        private const string FreightRfqModuleName = "Freight RFQ";   // must match AppNotification.NotificationConfig.ModuleName

        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public CreateFreightRfqCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _appDataMiscLookup = appDataMiscLookup;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateFreightRfqCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<FreightRfqHeader>(request);

            // One quotation row per selected transporter; quoted rate stays null until the reply is entered.
            entity.Quotations = request.Transporters.Select(t => new FreightRfqQuotation
            {
                TransporterId = t.TransporterId,
                TransportDetailId = t.TransportDetailId,
                RateBasisId = t.RateBasisId,
                VehicleNo = t.VehicleNo,
                TransportModeName = t.TransportModeName,
                VehicleTypeName = t.VehicleTypeName,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            }).ToList();

            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("UnitId is not available for the current user.");

            // FreightRfqNumber from the document sequence (Finance.TransactionTypeMaster "Freight RFQ Quotation").
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeFreightRfqQuotation, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new InvalidOperationException("No transaction type configured for Freight RFQ.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            entity.FreightRfqNumber = sequences.Count > 0
                ? sequences[^1]
                : throw new InvalidOperationException("No document sequence configured for Freight RFQ.");

            var newId = await _commandRepository.CreateAsync(entity, transactionTypeId, cancellationToken);

            // Email the RFQ to every selected transporter (their own "Freight RFQ" notification config).
            await NotifyTransportersAsync(entity, unitId, newId, request.Transporters, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "FREIGHTRFQ_CREATE",
                actionName: entity.FreightRfqNumber,
                details: $"Freight RFQ '{entity.FreightRfqNumber}' created successfully with Id {newId}.",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Freight RFQ created successfully.",
                Data = newId
            };
        }

        private async Task NotifyTransportersAsync(
            FreightRfqHeader header, int unitId, int rfqId,
            IReadOnlyList<FreightRfqTransporterInputDto> transporters, CancellationToken ct)
        {
            var withEmail = transporters.Where(t => !string.IsNullOrWhiteSpace(t.Email)).ToList();
            if (withEmail.Count == 0)
                return;

            var eventType = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                NotificationEnum.NotificationEvent, NotificationEnum.Create);
            var eventTypeId = eventType?.Id ?? 0;
            var createdByName = _ipAddressService.GetUserName() ?? string.Empty;

            foreach (var t in withEmail)
            {
                var correlationId = Guid.NewGuid();
                var @event = new NotificationCreatedEvent
                {
                    CorrelationId = correlationId,
                    CreatedByName = createdByName,
                    UnitId = unitId,
                    ModuleName = FreightRfqModuleName,
                    EventTypeId = eventTypeId,
                    Email = t.Email!.Trim(),
                    ccMail = string.Empty,
                    Mobile = t.Mobile?.Trim() ?? string.Empty,
                    param1 = string.IsNullOrWhiteSpace(t.Name) ? "Transporter" : t.Name!.Trim(),
                    param2 = header.FreightRfqNumber,
                    param3 = header.RfqDate,
                    param4 = header.SourceStation,
                    param5 = header.DestinationStation,
                    param6 = header.TotalQuantity.ToString("0.###"),
                    param7 = header.TotalBaleCount.ToString(),
                    param8 = header.RfqValidTill?.ToString(NotificationEnum.DateFormat) ?? string.Empty,
                    param9 = createdByName,
                    param10 = string.Empty,
                    ModuleTransactionId = rfqId,
                    ModuleTypeName = FreightRfqModuleName
                };

                await _outboxEventPublisher.ScheduleAsync(@event, correlationId, ct);
            }
        }
    }
}
