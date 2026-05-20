using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using PurchaseManagement.Domain.PurchaseOrder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create
{
    public class CreateServicePurchaseOrderCommandHandler : IRequestHandler<CreateServicePoCommand, int>
    {

        private readonly IMapper _mapper;
        private readonly IServicePurchaseOrderCommandRepository _servicePurchaseOrderCommandRepository;
        private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
        private readonly IIPAddressService _ip;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IUnitLookup _unitLookup;

        private readonly ILogger<CreateServicePurchaseOrderCommandHandler> _logger;

        public CreateServicePurchaseOrderCommandHandler(IMapper mapper, IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository, IIPAddressService ip, ITimeZoneService tz, IDocumentSequenceLookup documentSequenceLookup
          , IOutboxEventPublisher outboxEventPublisher, IServicePurchaseOrderQueryRepository poQuery, IUnitLookup unitLookup, ILogger<CreateServicePurchaseOrderCommandHandler> logger)
        {

            _mapper = mapper;
            _servicePurchaseOrderCommandRepository = servicePurchaseOrderCommandRepository;
            _ip = ip;
            _timeZoneService = tz;
            _documentSequenceLookup = documentSequenceLookup;
            _outboxEventPublisher = outboxEventPublisher;
            _servicePurchaseOrderQueryRepository = poQuery;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<int> Handle(CreateServicePoCommand request, CancellationToken ct)
        {
            var dto = request.Data;
            // 1) Map DTO -> aggregate
            var entity = _mapper.Map<PurchaseOrderHeader>(request.Data);

            // 2) Audit + PO number
            entity.UnitId = _ip.GetUnitId() ?? 0;
            var units = await _unitLookup.GetAllUnitAsync();
            var unitLookupDict = units.ToDictionary(u => u.UnitId, u => (u.ShortName ?? string.Empty).Trim());
            unitLookupDict.TryGetValue(entity.UnitId, out var unitCode);
            // Generate PONumber from DocumentSequence — same pattern as Local/Import/Contract POs.
            // Replaces the legacy MAX+1 generator which had an FY-collision bug (FY-scoped MAX vs
            // FY-agnostic uniqueness check would re-issue an existing PONumber on the first create
            // of each new FY). DocumentSequence is atomic, per-FY, and never collides.
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeSPO, MiscEnumEntity.ModulePurchase, entity.UnitId)
                ?? throw new InvalidOperationException(
                    $"No '{MiscEnumEntity.TransactionTypeSPO}' transaction type configured for UnitId {entity.UnitId}.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            entity.PONumber = sequences.Count > 0
                ? sequences[^1]
                : throw new InvalidOperationException(
                    $"No document sequence configured for '{MiscEnumEntity.TransactionTypeSPO}' (TransactionTypeId {transactionTypeId}).");
            entity.CreatedBy = _ip.GetUserId();
            entity.CreatedByName = _ip.GetUserName();
            entity.CreatedIP = _ip.GetSystemIPAddress();
            entity.CreatedDate = DateTimeOffset.UtcNow;

            // ---- Documents (rename on disk + attach to aggregate)

            if (dto?.Documents != null && dto.Documents.Any())
            {
                // Keep real docs only
                dto.Documents = dto.Documents
                    .Where(d => d.DocumentId != 0 && !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (dto.Documents.Any())
                {
                    var baseDirectory = MiscEnumEntity.DocumentPath;
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
                    EnsureDirectoryExists(uploadPath);

                    foreach (var doc in dto.Documents)
                    {
                        if (string.IsNullOrWhiteSpace(doc.FileName)) continue;

                        var oldFilePath = Path.Combine(uploadPath, doc.FileName);
                        if (!File.Exists(oldFilePath)) continue;

                        var newFileName = $"{entity.PONumber}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
                        var newFilePath = Path.Combine(uploadPath, newFileName);

                        try
                        {
                            File.Move(oldFilePath, newFilePath, overwrite: true);
                            doc.FileName = newFileName;
                            if (doc.UploadedDate == default) doc.UploadedDate = DateTimeOffset.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed renaming PO document to {NewFileName}", newFileName);
                        }
                    }

                    // Attach to entity so EF persists the graph
                    entity.PurchaseDocumentTypes = dto.Documents.Select(d => new PurchaseDocument
                    {
                        DocumentId = d.DocumentId,
                        FileName = d.FileName,
                        UploadedDate = d.UploadedDate
                    }).ToList();
                }
            }

            // 3) Persist
            // Pass transactionTypeId so the repo's atomic tx also advances
            // Finance.DocumentSequence.DocNo for SPO (mirrors Local PO numbering).
            var id = await _servicePurchaseOrderCommandRepository.CreateAsync(entity, ct, transactionTypeId);
            if (id <= 0) return id;

            // 4) Reload aggregate with real IDs for workflow packaging
            var agg = await _servicePurchaseOrderQueryRepository.GetByIdAsync(id, ct)
                    ?? throw new InvalidOperationException($"Service PO {id} not found after create.");

            // 5) Build workflow payload
            var wf = _mapper.Map<CreatePOServiceReverseDto>(agg);
            wf.Lines = new();

            var payload = JsonSerializer.Serialize(wf);
            var correlationId = Guid.NewGuid();

            // 6) Schedule Outbox Event (SQL Transactional Outbox)
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ServicePO,
                ModuleTransactionId = id,
                Payload = payload
            };

            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, ct);

            _logger.LogInformation(
                "ServicePO created. Id={PoId}, CorrelationId={CorrelationId}",
                id, correlationId);

            return id;
        }

        private static void EnsureDirectoryExists(string path)
            {
                if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

    }
}
