using System.Globalization;
using System.IO;
using System.Text.Json;
using AutoMapper;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Dtos.Lookups.Inventory;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create
{
    public class CreateRfqCommandHandler : IRequestHandler<CreateRfqCommand, int>
    {
        private readonly ITimeZoneService _timeZoneService;
        private readonly IRfqCommandRepository _rfqRepo;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ip;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly ILogger<CreateRfqCommandHandler> _logger;
        private readonly IPurchaseIndentCommand _indentRepo;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;
        private readonly IRfqAttachmentFileStorage _attachmentStorage;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public CreateRfqCommandHandler(
            IRfqCommandRepository rfqRepo,
            IMapper mapper,
            IIPAddressService ip,
            IOutboxEventPublisher outboxEventPublisher,
            ILogger<CreateRfqCommandHandler> logger,
            IItemLookup itemLookup,
            IUOMLookup uOMLookup, ITimeZoneService timeZoneService, IPurchaseIndentCommand indentRepo,
            IAppDataMiscMasterLookup appDataMiscLookup,
            IRfqAttachmentFileStorage attachmentStorage,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _rfqRepo = rfqRepo ?? throw new ArgumentNullException(nameof(rfqRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _outboxEventPublisher = outboxEventPublisher ?? throw new ArgumentNullException(nameof(outboxEventPublisher));
            _itemLookup = itemLookup ?? throw new ArgumentNullException(nameof(itemLookup));
            _uomLookup = uOMLookup ?? throw new ArgumentNullException(nameof(uOMLookup));
            _logger = logger ?? NullLogger<CreateRfqCommandHandler>.Instance;
            _timeZoneService = timeZoneService ?? throw new ArgumentNullException(nameof(timeZoneService));
            _indentRepo = indentRepo;
            _appDataMiscLookup = appDataMiscLookup;
            _attachmentStorage = attachmentStorage ?? throw new ArgumentNullException(nameof(attachmentStorage));
            _documentSequenceLookup = documentSequenceLookup ?? throw new ArgumentNullException(nameof(documentSequenceLookup));
        }
        
        public async Task<int> Handle(CreateRfqCommand request, CancellationToken ct)
        {
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            TimeZoneInfo systemTimeZone;
            try
            {
                systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(systemTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                if (string.Equals(systemTimeZoneId, "India Standard Time", StringComparison.OrdinalIgnoreCase))
                    systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                else
                    systemTimeZone = TimeZoneInfo.Local;

            }

            // convert to local time
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, systemTimeZone);
            // 1) Map header
            var rfq = _mapper.Map<RfqMaster>(request);

            // 2) Ensure items
            if (rfq.Items is null || rfq.Items.Count == 0)
            {
                rfq.Items = request.Items.Select(i => new RfqItem
                {
                    ItemId = i.ItemId,
                    HsnId = i.HsnId,
                    Quantity = i.Qty,
                    UomId = i.UomId
                }).ToList();
            }

            // 2a) UOM name map (UomId -> Name/Code)
            var uomMap = new Dictionary<int, string>();
            try
            {
                var uomIds = rfq.Items.Select(x => x.UomId).Distinct().ToList();
                if (uomIds.Count > 0)
                {
                    var allUoms = await _uomLookup.GetAllAsync() ?? new List<UOMLookupDto>();
                    uomMap = allUoms
                        .Where(u => uomIds.Contains(u.Id))
                        .GroupBy(u => u.Id)
                        .ToDictionary(g => g.Key, g =>
                        {
                            var u = g.First();
                            return string.IsNullOrWhiteSpace(u.UOMName) ? (u.Code ?? u.Id.ToString()) : u.UOMName;
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UOM gRPC lookup failed; proceeding with UoM IDs.");
            }

            // 3) Ensure suppliers
            if (rfq.Suppliers is null || rfq.Suppliers.Count == 0)
            {
                rfq.Suppliers = request.Suppliers.Select(s => new RfqSupplier
                {
                    SupplierId = s.SupplierId > 0 ? s.SupplierId : null,
                    Name = s.Name,
                    Mobile = s.Mobile,
                    GSTNumber = s.Gst,
                    Email = string.IsNullOrWhiteSpace(s.Email) ? null : new PurchaseManagement.Domain.Entities.ValueObjects.EmailAddress(s.Email)
                }).ToList();
            }

            // 3a) Attachments — move staged files to permanent and attach to aggregate.
            // Wrapped with persist (step 5) in a try/catch so any failure rolls back moved files.
            var movedPaths = new List<string>();
            int id;
            try
            {
                if (request.Attachments?.Count > 0)
                {
                    foreach (var att in request.Attachments)
                    {
                        var permanentPath = await _attachmentStorage.MoveStagedToPermanentAsync(att.FileName, ct);
                        movedPaths.Add(permanentPath);

                        rfq.Attachments.Add(new PurchaseManagement.Domain.Entities.Quotation.RfqEntry.RfqAttachment
                        {
                            FileName = Path.GetFileName(permanentPath),
                            OriginalFileName = att.OriginalFileName,
                            FilePath = permanentPath,
                            FileType = att.FileType,
                            FileSize = att.FileSize,
                            CreatedBy = _ip.GetUserId(),
                            CreatedByName = _ip.GetUserName(),
                            CreatedIP = _ip.GetSystemIPAddress(),
                            CreatedDate = now,
                        });
                    }
                }

                // 4) Audit
                rfq.UnitId = _ip.GetUnitId() ?? 0;

                // Generate RfqCode from DocumentSequence
                var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                    MiscEnumEntity.TransactionTypeRFQ, MiscEnumEntity.ModulePurchase, rfq.UnitId ?? 0)
                    ?? throw new InvalidOperationException("No transaction type configured for RFQ.");
                var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
                rfq.RfqCode = sequences.Count > 0
                    ? sequences[^1]
                    : throw new InvalidOperationException("No document sequence configured for RFQ.");

                rfq.CreatedBy = _ip.GetUserId();
                rfq.CreatedByName = _ip.GetUserName();
                rfq.CreatedIP = _ip.GetSystemIPAddress();
                rfq.CreatedDate = now;

                // 5) Persist
                id = await _rfqRepo.CreateAsync(rfq, transactionTypeId, ct);
                if (id <= 0) throw new InvalidOperationException("RFQ creation failed.");
            }
            catch
            {
                foreach (var path in movedPaths)
                {
                    try { await _attachmentStorage.DeleteAsync(path, CancellationToken.None); }
                    catch { /* best-effort cleanup */ }
                }
                throw;
            }
            if (request.IndentDetailIds?.Count > 0)
            {
                var updated = await _indentRepo.UpdateRFQStatusAsync(request.IndentDetailIds);

                _logger.LogInformation("RFQ {Id}: IsRFQDone updated for {Count} IndentDetail rows.", id, request.IndentDetailIds.Count);
            }

            // 5a) Item code/name via gRPC
            var itemMap = new Dictionary<int, (string Code, string Name)>();
            try
            {
                var itemIds = rfq.Items.Select(x => x.ItemId).Distinct().ToList();
                if (itemIds.Count > 0)
                {
                    var lite = await _itemLookup.GetByIdsAsync(itemIds, ct) ?? new List<ItemLookupDto>();
                    itemMap = lite
                        .GroupBy(i => i.Id)
                        .ToDictionary(g => g.Key, g =>
                        {
                            var i = g.First();
                            return (i.ItemCode ?? string.Empty, i.ItemName ?? string.Empty);
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Item gRPC lookup failed; proceeding without names.");
            }

            // 6) Build rows JSON (keys must match preset): item, qty, uom, requiredBy
            var rows = new List<Dictionary<string, object?>>();
            foreach (var i in rfq.Items)
            {
                itemMap.TryGetValue(i.ItemId, out var t);
                var display = (string.IsNullOrWhiteSpace(t.Name) && string.IsNullOrWhiteSpace(t.Code))
                                ? $"#{i.ItemId}"
                                : $"{t.Code} – {t.Name}";
                var uomName = uomMap.TryGetValue(i.UomId, out var nm) ? nm : i.UomId.ToString();

                rows.Add(new Dictionary<string, object?>
                {
                    ["item"] = display,
                    ["qty"] = i.Quantity,
                    ["uom"] = uomName
                    //, ["requiredBy"] = rfq.LastSubmitDate?.ToString("dd-MMM-yyyy") ?? ""
                });
            }

            var rowsJson = JsonSerializer.Serialize(
                rows,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


            DateTimeOffset dueDate = rfq.LastSubmitDate.HasValue
                ? StartOfDay(rfq.LastSubmitDate.Value, systemTimeZone)
                : now;

            var contactName = rfq.CreatedByName ?? "Purchasing";
            var createdDateStr = now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            // 7) Publish one email per supplier
            try
            {
                var notifEventMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                    NotificationEnum.NotificationEvent, NotificationEnum.Create);

                foreach (var s in request.Suppliers)
                {
                    var recipientName = string.IsNullOrWhiteSpace(s.Name) ? "Supplier" : s.Name.Trim();
                    var email = s.Email?.Trim() ?? string.Empty;
                    var mobile = s.Mobile?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(email))
                    {
                        _logger.LogWarning("RFQ {Id}: supplier '{Supplier}' has no email, skipping.", id, recipientName);
                        continue;
                    }

                    var correlationId = Guid.NewGuid();
                    var @event = new NotificationCreatedEvent
                    {
                        CorrelationId = correlationId,
                        CreatedByName = contactName,
                        UnitId = rfq.UnitId ?? _ip.GetUnitId() ?? 0,
                        ModuleName = "RFQ",
                        EventTypeId = notifEventMisc?.Id ?? 0,
                        Email = email,
                        ccMail = "",
                        Mobile = mobile,
                        // Template tokens
                        param1 = recipientName,
                        param2 = rfq.RfqCode,
                        param3 = dueDate,
                        param4 = "",
                        param5 = createdDateStr,
                        param6 = "",
                        param7 = contactName,
                        param8 = "",
                        param9 = "",
                        param10 = rowsJson,
                        ModuleTransactionId = 0,
                        ModuleTypeName = "RFQ"
                    };

                    await _outboxEventPublisher.ScheduleAsync(@event, correlationId, ct);

                    _logger.LogInformation("📨 RFQ email queued for '{Recipient}' <{Email}>. RFQ {Id}/{Code} (CorrId: {Corr})",
                        recipientName, email, id, rfq.RfqCode, correlationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish NotificationCreatedEvent(s) for RFQ {Id}", id);
            }

            return id;
        }
        static DateTimeOffset StartOfDay(DateOnly d, TimeZoneInfo tz)
        {
            var local = d.ToDateTime(TimeOnly.MinValue); 
            var offset = tz.GetUtcOffset(local);
            return new DateTimeOffset(local, offset);
        }

    }
}
