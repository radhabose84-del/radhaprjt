using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment
{
    public class CreateSalesQuotationAmendmentCommandHandler
        : IRequestHandler<CreateSalesQuotationAmendmentCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesQuotationAmendmentCommandRepository _commandRepository;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateSalesQuotationAmendmentCommandHandler> _logger;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;
        private readonly IOfficerAgentUserLookup _officerAgentUserLookup;

        public CreateSalesQuotationAmendmentCommandHandler(
            ISalesQuotationAmendmentCommandRepository commandRepository,
            IOutboxEventPublisher outboxEventPublisher,
            IIPAddressService ipAddressService,
            IMediator mediator,
            ILogger<CreateSalesQuotationAmendmentCommandHandler> logger,
            IAppDataMiscMasterLookup appDataMiscLookup,
            IOfficerAgentUserLookup officerAgentUserLookup)
        {
            _commandRepository = commandRepository;
            _outboxEventPublisher = outboxEventPublisher;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _logger = logger;
            _appDataMiscLookup = appDataMiscLookup;
            _officerAgentUserLookup = officerAgentUserLookup;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateSalesQuotationAmendmentCommand request, CancellationToken cancellationToken)
        {
            // Fetch the Quotation entity (with details) to snapshot Old* values and get RevisionNumber
            var sqHeader = await _commandRepository.GetSalesQuotationEntityAsync(request.SalesQuotationHeaderId);
            if (sqHeader == null)
                throw new ExceptionRules("Sales quotation not found.");

            // Derive AmendmentNo: {QuotationNo}/AMD/{RevisionNumber}
            var revisionNumber = sqHeader.RevisionNumber + 1;
            var amendmentNo = $"{sqHeader.QuotationNo}/AMD/{revisionNumber}";

            var header = new SalesQuotationAmendmentHeader
            {
                SalesQuotationHeaderId = request.SalesQuotationHeaderId,
                UnitId = _ipAddressService.GetUnitId() ?? 0,
                AmendmentNo = amendmentNo,
                RevisionNumber = revisionNumber,
                AmendmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Reason = request.Reason,
                FreightCharges = request.FreightCharges,
                OtherCharges = request.OtherCharges,
                TotalBasicAmount = request.TotalBasicAmount,
                TotalDiscount = request.TotalDiscount,
                NetTaxableAmount = request.NetTaxableAmount,
                TotalTax = request.TotalTax,
                GrandTotal = request.GrandTotal,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            // Build detail rows from the command — snapshot Old* values from Quotation detail lines
            // ChangeType is auto-derived: any New* value → "Modified", all null → "Removed"
            var details = new List<SalesQuotationAmendmentDetail>();
            foreach (var dto in request.AmendmentDetails ?? [])
            {
                var sqDetail = sqHeader.SalesQuotationDetails?
                    .FirstOrDefault(d => d.Id == dto.SalesQuotationDetailId);
                if (sqDetail == null)
                    throw new ExceptionRules($"Sales quotation detail Id {dto.SalesQuotationDetailId} not found.");

                var hasNewValues = dto.NewItemId.HasValue
                    || dto.NewQuantity.HasValue
                    || dto.NewExMillRate.HasValue
                    || dto.NewDiscount.HasValue
                    || dto.NewHSNId.HasValue
                    || dto.NewTaxPercentage.HasValue;

                var changeType = hasNewValues ? "Modified" : "Removed";

                details.Add(new SalesQuotationAmendmentDetail
                {
                    ChangeType = changeType,
                    SalesQuotationDetailId = dto.SalesQuotationDetailId,
                    OldItemId = sqDetail.ItemId,
                    OldQuantity = sqDetail.Quantity,
                    OldExMillRate = sqDetail.ExMillRate,
                    OldDiscount = sqDetail.Discount,
                    OldHSNId = sqDetail.HSNId,
                    OldTaxPercentage = sqDetail.TaxPercentage,
                    NewItemId = dto.NewItemId,
                    NewQuantity = dto.NewQuantity,
                    NewExMillRate = dto.NewExMillRate,
                    NewDiscount = dto.NewDiscount,
                    NewHSNId = dto.NewHSNId,
                    NewTaxPercentage = dto.NewTaxPercentage,
                    NetRate = dto.NetRate,
                    TotalAmount = dto.TotalAmount,
                    TaxAmount = dto.TaxAmount
                });
            }

            var newId = await _commandRepository.CreateAsync(header, details);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESQUOTATIONAMENDMENT_CREATE",
                actionName: amendmentNo,
                details: $"Sales Quotation Amendment '{amendmentNo}' created for Quotation Id {request.SalesQuotationHeaderId} with Id {newId}.",
                module: "SalesQuotationAmendment"), cancellationToken);

            if (newId <= 0)
                throw new ExceptionRules("Sales Quotation Amendment Creation Failed.");

            // ------------------- 2) InApp notification to role-based users (approval) -------------------
            var notifEventMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                NotificationEnum.NotificationEvent, NotificationEnum.Create);

            try
            {
                List<int>? overrideUserIds = null;
                var moUserId = await _officerAgentUserLookup.GetMarketingOfficerReportToUserIdAsync(_ipAddressService.GetUserId(), cancellationToken);

                if (moUserId.HasValue && moUserId.Value > 0)
                {
                    overrideUserIds = new List<int> { moUserId.Value };
                }
                else
                {
                    _logger.LogWarning(
                        "No ReportTo UserId resolved for current user on Sales Quotation Amendment {AmendmentNo} (Id={Id}). Skipping InApp notification.",
                        amendmentNo, newId);
                }

                if (overrideUserIds != null)
                {
                    var inAppCorrelationId = Guid.NewGuid();
                    var inAppEvent = new NotificationCreatedEvent
                    {
                        CorrelationId = inAppCorrelationId,
                        CreatedByName = _ipAddressService.GetUserName() ?? string.Empty,
                        UnitId = _ipAddressService.GetUnitId() ?? 0,
                        ModuleName = "SalesQuotationAmendment",
                        EventTypeId = notifEventMisc?.Id ?? 0,
                        Email = "",
                        ccMail = "",
                        Mobile = "",
                        param1 = amendmentNo,
                        param2 = sqHeader.QuotationNo ?? "",
                        param3 = DateTimeOffset.UtcNow,
                        param4 = "",
                        param5 = "",
                        param6 = "",
                        param7 = "",
                        param8 = "",
                        param9 = "",
                        param10 = "",
                        OverrideTargetUserIds = overrideUserIds
                    };

                    await _outboxEventPublisher.ScheduleWithoutSaveAsync(inAppEvent, inAppCorrelationId, cancellationToken);
                    _logger.LogInformation(
                        "InApp notification queued for role-based users. Amendment {AmendmentNo} (CorrId: {Corr})",
                        amendmentNo, inAppCorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish InApp NotificationCreatedEvent for Sales Quotation Amendment {Id}", newId);
            }

            // ------------------- 3) Workflow approval request -------------------
            // Fetch entity for workflow payload
            var workFlowEntity = await _commandRepository.GetByIdAmendmentWorkFlowAsync(newId);
            var reverseMap = new CreateSalesQuotationAmendmentReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };
            string serializedPayload = JsonSerializer.Serialize(reverseMap);

            // Schedule Outbox Event (SQL Transactional Outbox)
            var correlationId = Guid.NewGuid();
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TransactionTypeSalesQuotationAmendment,
                ModuleTransactionId = newId,
                Payload = serializedPayload
            };
            await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);

            // ------------------- Atomic commit: all outbox events saved together -------------------
            await _outboxEventPublisher.SavePendingAsync(cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Quotation Amendment created successfully.",
                Data = newId
            };
        }
    }
}
