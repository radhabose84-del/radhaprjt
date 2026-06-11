using Contracts.Common;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Domain.Events;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations
{
    public class SaveFreightRfqQuotationsCommandHandler : IRequestHandler<SaveFreightRfqQuotationsCommand, ApiResponseDTO<int>>
    {
        private const string FreightRfqModuleName = "Freight RFQ";

        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IFreightRfqQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public SaveFreightRfqQuotationsCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IFreightRfqQueryRepository queryRepository,
            IMediator mediator,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _appDataMiscLookup = appDataMiscLookup;
        }

        public async Task<ApiResponseDTO<int>> Handle(SaveFreightRfqQuotationsCommand request, CancellationToken cancellationToken)
        {
            var rows = request.Quotations.Select(q => new FreightRfqQuotation
            {
                Id = q.Id,
                TransporterId = q.TransporterId,
                TransportDetailId = q.TransportDetailId,
                RateBasisId = q.RateBasisId,
                QuotedRate = q.QuotedRate,
                NoOfVehicles = q.NoOfVehicles,
                VehicleNo = q.VehicleNo,
                TransportModeName = q.TransportModeName,
                VehicleTypeName = q.VehicleTypeName,
                Remarks = q.Remarks,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            }).ToList();

            var result = await _commandRepository.SaveQuotationsAsync(request.FreightRfqId, rows);

            // Email any newly-added transporters (rows with Id = 0) — the others were already mailed.
            await NotifyNewTransportersAsync(request, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "FREIGHTRFQ_QUOTATIONS_SAVE",
                actionName: request.FreightRfqId.ToString(),
                details: $"Freight RFQ {request.FreightRfqId} quotations saved ({rows.Count} rows).",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quotations saved successfully.",
                Data = result
            };
        }

        private async Task NotifyNewTransportersAsync(SaveFreightRfqQuotationsCommand request, CancellationToken ct)
        {
            var newOnes = request.Quotations
                .Where(q => q.Id == 0 && !string.IsNullOrWhiteSpace(q.Email))
                .ToList();
            if (newOnes.Count == 0)
                return;

            var header = await _queryRepository.GetByIdAsync(request.FreightRfqId);
            if (header == null)
                return;

            var eventType = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                NotificationEnum.NotificationEvent, NotificationEnum.Create);
            var eventTypeId = eventType?.Id ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var createdByName = _ipAddressService.GetUserName() ?? string.Empty;

            foreach (var t in newOnes)
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
                    param2 = header.FreightRfqNumber ?? string.Empty,
                    param3 = header.RfqDate,
                    param4 = header.SourceStation ?? string.Empty,
                    param5 = header.DestinationStation ?? string.Empty,
                    param6 = header.TotalQuantity.ToString("0.###"),
                    param7 = header.TotalBaleCount.ToString(),
                    param8 = header.RfqValidTill?.ToString(NotificationEnum.DateFormat) ?? string.Empty,
                    param9 = createdByName,
                    param10 = string.Empty,
                    ModuleTransactionId = request.FreightRfqId,
                    ModuleTypeName = FreightRfqModuleName
                };

                await _outboxEventPublisher.ScheduleAsync(@event, correlationId, ct);
            }
        }
    }
}
