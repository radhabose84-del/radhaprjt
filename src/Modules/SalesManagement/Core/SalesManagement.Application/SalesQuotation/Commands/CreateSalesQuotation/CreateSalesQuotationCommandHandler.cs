using System.Globalization;
using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation
{
    public class CreateSalesQuotationCommandHandler : IRequestHandler<CreateSalesQuotationCommand, int>
    {
        private readonly ISalesQuotationCommandRepository _commandRepository;
        private readonly ISalesQuotationQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IPartyDetailLookup _partyDetailLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;
        private readonly ILogger<CreateSalesQuotationCommandHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOfficerAgentUserLookup _officerAgentUserLookup;

        public CreateSalesQuotationCommandHandler(
            ISalesQuotationCommandRepository commandRepository,
            ISalesQuotationQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IPartyDetailLookup partyDetailLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            ILogger<CreateSalesQuotationCommandHandler> logger,
            IAppDataMiscMasterLookup appDataMiscLookup,
            IConfiguration configuration, IOfficerAgentUserLookup officerAgentUserLookup)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _partyDetailLookup = partyDetailLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
            _logger = logger; _appDataMiscLookup = appDataMiscLookup; _officerAgentUserLookup = officerAgentUserLookup;
        }


        public async Task<int> Handle(CreateSalesQuotationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesQuotationHeader>(request);

            // Set default StatusId to 'Pending'
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.InvoiceApprovalStatus, MiscEnumEntity.InvoiceStatusPending);
            entity.StatusId = pendingStatus?.Id;

            // Generate QuotationNo from DocumentSequence
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeSalesQuotation,
                MiscEnumEntity.ModuleSales,
                unitId);

            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Sales Quotation' not found. Please configure it in Transaction Type Master.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var quotationNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.QuotationNo = quotationNo
                ?? throw new ExceptionRules("No document sequence configured for Sales Quotation.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESQUOTATION_CREATE",
                actionName: quotationNo,
                details: $"Sales Quotation '{quotationNo}' created successfully with Id {newId}.",
                module: "SalesQuotation");
            await _mediator.Publish(auditEvent, cancellationToken);

            if (newId <= 0)
                throw new ExceptionRules("Sales Quotation Creation Failed.");
          
            // ------------------- 3) Workflow approval request -------------------
            var workFlowEntity = await _commandRepository.GetByIdSalesQuotationWorkFlowAsync(newId);
            workFlowEntity.UnitId = unitId;
            var reverseMap = new CreateSalesQuotationReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };
            string serializedPayload = JsonSerializer.Serialize(reverseMap);

            var correlationId = Guid.NewGuid();
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TransactionTypeSalesQuotation,
                ModuleTransactionId = newId,
                Payload = serializedPayload
            };

            await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);

            // ------------------- Notification setup (shared by Email + InApp) -------------------
            var customer = await _partyDetailLookup.GetByIdAsync(request.CustomerId, cancellationToken);
            var notifEventMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                NotificationEnum.NotificationEvent, NotificationEnum.Create);
            var customerName = customer?.PartyName ?? "";

            // ------------------- 1) Email notification to customer (item-wise quotation) -------------------
            try
            {
                if (customer != null && !string.IsNullOrWhiteSpace(customer.EmailID))
                {
                    // Build item rows JSON — data only, no HTML (table rendering handled by Sp_RenderHtmlTableFromTemplate)
                    var rowsJson = "";
                    if (request.SalesQuotationDetails != null && request.SalesQuotationDetails.Count > 0)
                    {
                        var itemIds = request.SalesQuotationDetails.Select(d => d.ItemId).Distinct().ToList();
                        var hsnIds = request.SalesQuotationDetails.Select(d => d.HSNId).Distinct().ToList();

                        var itemList = await _itemLookup.GetByIdsAsync(itemIds, cancellationToken);
                        var itemDict = itemList.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                        var hsnList = await _hsnLookup.GetByIdsAsync(hsnIds, cancellationToken);
                        var hsnDict = hsnList.ToDictionary(h => h.Id, h => h.HSNCode);

                        var sno = 1;
                        var rows = new List<Dictionary<string, object?>>();
                        foreach (var detail in request.SalesQuotationDetails)
                        {
                            itemDict.TryGetValue(detail.ItemId, out var itemInfo);
                            hsnDict.TryGetValue(detail.HSNId, out var hsnCode);

                            var itemDesc = !string.IsNullOrWhiteSpace(itemInfo.ItemName)
                                ? $"{itemInfo.ItemCode} – {itemInfo.ItemName}"
                                : $"#{detail.ItemId}";

                            rows.Add(new Dictionary<string, object?>
                            {
                                ["sNo"] = sno++,
                                ["item"] = itemDesc,
                                ["hsn"] = hsnCode ?? "",
                                ["qty"] = detail.Quantity,
                                ["rate"] = detail.ExMillRate,
                                ["discount"] = detail.Discount,
                                ["amount"] = detail.TotalAmount,
                                ["gstPct"] = detail.TaxPercentage,
                                ["gstAmt"] = detail.TaxAmount
                            });
                        }

                        rowsJson = JsonSerializer.Serialize(rows,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    }

                    var quotationDateStr = request.QuotationDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    var validityDateStr = request.ValidityDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    var emailCorrelationId = Guid.NewGuid();
                    var emailEvent = new NotificationCreatedEvent
                    {
                        CorrelationId = emailCorrelationId,
                        CreatedByName = _ipAddressService.GetUserName() ?? string.Empty,
                        UnitId = _ipAddressService.GetUnitId() ?? 0,
                        ModuleName = "SalesQuotation",
                        EventTypeId = notifEventMisc?.Id ?? 0,
                        Email = customer.EmailID,
                        ccMail = "",
                        Mobile = customer.MobileNo ?? "",
                        param1 = quotationNo ?? "",
                        param2 = customerName,
                        param3 = DateTimeOffset.UtcNow,
                        param4 = "",
                        param5 = quotationDateStr,
                        param6 = validityDateStr,
                        param7 = entity.GrandTotal.ToString("N2"),
                        param8 = entity.TotalTax.ToString("N2"),
                        param9 = customer.GSTNumber ?? "",
                        param10 = rowsJson
                    };

                    await _outboxEventPublisher.ScheduleWithoutSaveAsync(emailEvent, emailCorrelationId, cancellationToken);
                    _logger.LogInformation(
                        "Email notification queued for customer '{CustomerName}' <{Email}>. Quotation {QuotationNo}, {ItemCount} items (CorrId: {Corr})",
                        customerName, customer.EmailID, quotationNo,
                        request.SalesQuotationDetails?.Count ?? 0, emailCorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish email NotificationCreatedEvent for Sales Quotation {Id}", newId);
            }

            // ------------------- 2) InApp notification to role-based users (approval) -------------------
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
                        "No ReportTo UserId resolved for current user on Sales Quotation {QuotationNo} (Id={Id}). Skipping InApp notification.",
                        quotationNo, newId);
                }

                if (overrideUserIds != null)
                {
                    var inAppCorrelationId = Guid.NewGuid();
                    var inAppEvent = new NotificationCreatedEvent
                    {
                        CorrelationId = inAppCorrelationId,
                        CreatedByName = _ipAddressService.GetUserName() ?? string.Empty,
                        UnitId = _ipAddressService.GetUnitId() ?? 0,
                        ModuleName = "SalesQuotation",
                        EventTypeId = notifEventMisc?.Id ?? 0,
                        Email = "",
                        ccMail = "",
                        Mobile = "",
                        param1 = quotationNo ?? "",
                        param2 = customerName,
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
                        "InApp notification queued for role-based users. Quotation {QuotationNo} (CorrId: {Corr})",
                        quotationNo, inAppCorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish InApp NotificationCreatedEvent for Sales Quotation {Id}", newId);
            }

            // ------------------- Atomic commit: all outbox events saved together -------------------
            await _outboxEventPublisher.SavePendingAsync(cancellationToken);

            return newId;
        }
    }
}
