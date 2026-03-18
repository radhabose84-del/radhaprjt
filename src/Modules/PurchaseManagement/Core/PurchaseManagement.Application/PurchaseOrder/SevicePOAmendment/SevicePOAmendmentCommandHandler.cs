using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.PurchaseOrder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.SevicePOAmendment
{
    public class SevicePOAmendmentCommandHandler : IRequestHandler<SevicePOAmendmentCommand, int>
    {

        private readonly IServicePurchaseOrderQueryRepository _qry;
        private readonly IServicePurchaseOrderCommandRepository _cmd;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IIPAddressService _ip;
        private readonly ITimeZoneService _tz;
        private readonly IMapper _mapper;
        private readonly ILogger<SevicePOAmendmentCommandHandler> _logger;
        private readonly IOutboxEventPublisher _outboxEventPublisher;


        public SevicePOAmendmentCommandHandler(
            IServicePurchaseOrderQueryRepository qry,
            IServicePurchaseOrderCommandRepository cmd,
            IMiscMasterQueryRepository misc,
            IMapper mapper,
            IIPAddressService ip,
            ITimeZoneService tz,
            IOutboxEventPublisher outboxEventPublisher,
            ILogger<SevicePOAmendmentCommandHandler> logger)
        {
            _qry = qry;
            _cmd = cmd;
            _misc = misc;
            _mapper = mapper;
            _ip = ip;
            _tz = tz;
            _outboxEventPublisher = outboxEventPublisher;
            _logger = logger;
        }

        public async Task<int> Handle(SevicePOAmendmentCommand request, CancellationToken ct)
        {
            var poId = request.Dto.Id;
            if (poId <= 0)
                throw new InvalidOperationException("Invalid Service PO Id for amendment.");

            // 1) Guard: existing DTO
            var existingDto = await _qry.GetServicePOByIdAsync(poId, ct)
                             ?? throw new InvalidOperationException($"Service PO {poId} not found.");

            // 2) Only approved can be amended
            var approved = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
            if (existingDto.StatusId != approved.Id)
                throw new InvalidOperationException("PO is not approved. Use Edit for Pending POs; Amendment is only for Approved POs.");

            // 3) Load existing entity (for EF-based AmendAsync)
            var existingEntity = await _cmd.GetAggregateAsync(poId, ct)
                                 ?? throw new InvalidOperationException($"Service PO entity {poId} not found.");

            // 4) Timezone + now
            var tzId = _tz.GetSystemTimeZone();
            if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
                tzId = "Asia/Kolkata";

            TimeZoneInfo tzi;
            try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
            catch { tzi = TimeZoneInfo.Local; }

            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

            // 5) Normalize revised DTO: revision, PONumber, remarks
            var revisedDto = request.Dto;
            revisedDto.RevisionNo = existingEntity.RevisionNo + 1;
            revisedDto.PONumber = BuildNextRevisionCode(existingEntity.PONumber, revisedDto.RevisionNo);
            revisedDto.AmendmentReason = (request.Dto.AmendmentReason ?? revisedDto.AmendmentReason)?.Trim();

            // 6) Document handling
            if (revisedDto.Documents is { Count: > 0 })
            {
                var baseDir = "ServicePoDocument";
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                foreach (var doc in revisedDto.Documents
                             .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName)))
                {
                    var oldPath = Path.Combine(uploadDir, doc.FileName!);
                    if (File.Exists(oldPath))
                    {
                        var finalName = $"{revisedDto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
                        var newPath = Path.Combine(uploadDir, finalName);

                        if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                File.Move(oldPath, newPath, overwrite: true);
                                doc.FileName = finalName;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Failed renaming Service PO document to {FinalName} for PO {PoNumber}",
                                    finalName, revisedDto.PONumber);
                            }
                        }
                    }

                    if (doc.UploadedDate == default)
                        doc.UploadedDate = now;
                }
            }

            // 7) Clear IDs so new rows are inserted for amended PO
            if (revisedDto.ServicePos != null)
            {
                foreach (var sh in revisedDto.ServicePos)
                {
                    sh.Id = null;
                    if (sh.Lines == null) continue;

                    foreach (var ln in sh.Lines)
                    {
                        ln.Id = null;
                        if (ln.Schedules == null) continue;

                        foreach (var sc in ln.Schedules)
                            sc.Id = null;
                    }
                }
            }

            if (revisedDto.PaymentTerms != null)
            {
                foreach (var t in revisedDto.PaymentTerms)
                    t.Id = null;
            }

            // 8) Map DTO -> entity
            var revisedEntity = _mapper.Map<PurchaseOrderHeader>(revisedDto);

            // Map docs to entity for EF
            if (revisedDto.Documents is { Count: > 0 })
            {
                revisedEntity.PurchaseDocumentTypes = revisedDto.Documents
                    .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
                    .Select(d => new PurchaseDocument
                    {
                        DocumentId = d.DocumentId,
                        FileName = d.FileName,
                        UploadedDate = d.UploadedDate
                    })
                    .ToList();
            }
            else
            {
                revisedEntity.PurchaseDocumentTypes = new List<PurchaseDocument>();
            }

            // 9) Amend in repo
            var newId = await _cmd.AmendAsync(existingEntity, revisedEntity, ct);

            _logger.LogInformation("Service PO amended. OldId={OldId}, NewId={NewId}", poId, newId);

            // 10) Reload for workflow payload
            var wfAgg = await _qry.GetByIdAsync(newId, ct)
                         ?? throw new InvalidOperationException($"Service PO {newId} not found after amend.");

            var wf = _mapper.Map<CreatePOServiceReverseDto>(wfAgg);
            wf.Lines = new();

            var payload = JsonSerializer.Serialize(wf);
            var correlationId = Guid.NewGuid();

            // 11) Schedule Outbox Event (SQL Transactional Outbox)
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ServicePO,
                ModuleTransactionId = newId,
                Payload = payload
            };

            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, ct);

            _logger.LogInformation(
                "ServicePO amendment workflow scheduled. OldId={OldId}, NewId={NewId}, CorrelationId={CorrelationId}",
                poId, newId, correlationId);

            return newId;
        }

        private static string BuildNextRevisionCode(string oldCode, int nextRev)
        {
            var idx = oldCode.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0 && idx + 2 < oldCode.Length && int.TryParse(oldCode[(idx + 2)..], out _))
                return oldCode[..idx] + $"-R{nextRev}";
            return $"{oldCode}-R{nextRev}";
        }

    }
}
