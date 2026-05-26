// PurchaseManagement.Application/PurchaseOrder/ImportPO/Command/ImportPOAmendment/ImportPOAmendmentCommandHandler.cs
using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment
{
    public sealed class ImportPOAmendmentCommandHandler : IRequestHandler<ImportPOAmendmentCommand, int>
    {
        private readonly IImportPOCommandRepository _cmd;
        private readonly IImportPOQueryRepository _qry;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ip;
        private readonly ITimeZoneService _tz;
        private readonly ILogger<ImportPOAmendmentCommandHandler> _logger;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IOutboxEventPublisher _outboxEventPublisher;

        public ImportPOAmendmentCommandHandler(
            IImportPOCommandRepository cmd,
            IImportPOQueryRepository qry,
            IMiscMasterQueryRepository misc,
            IMapper mapper,
            IIPAddressService ip,
            ITimeZoneService tz,
            ILogger<ImportPOAmendmentCommandHandler> logger,
            IDocumentSequenceLookup documentSequenceLookup,
            IOutboxEventPublisher outboxEventPublisher)
        {
            _cmd = cmd; _qry = qry; _misc = misc; _mapper = mapper;
            _ip = ip; _tz = tz; _logger = logger;
            _documentSequenceLookup = documentSequenceLookup;
            _outboxEventPublisher = outboxEventPublisher;
        }

        public async Task<int> Handle(ImportPOAmendmentCommand request, CancellationToken ct)
        {
            var dto = request.Data ?? throw new InvalidOperationException("Body required.");
            if (dto.Id <= 0) throw new InvalidOperationException("Invalid PO Id.");

            var existing = await _cmd.GetAggregateAsync(dto.Id, ct)
                           ?? throw new InvalidOperationException($"PO {dto.Id} not found.");

            // Only Approved → Amendment
            var approved = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
            if (existing.StatusId != approved.Id)
                throw new InvalidOperationException("PO is not approved. Use Edit API for Pending POs.");

            // Block if GRN exists
            if (await _qry.HasAnyGrnAsync(dto.Id, ct))
                throw new InvalidOperationException("GRN exists for this PO. Amendment not allowed.");

            // Timezone
            var tzId = _tz.GetSystemTimeZone();
            if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
                tzId = "Asia/Kolkata";
            TimeZoneInfo tzi; try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); } catch { tzi = TimeZoneInfo.Local; }
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

            // Compute next PONumber & revision on DTO
            var newRevisionNo = existing.RevisionNo + 1;
            dto.RevisionNo = newRevisionNo;
            dto.PONumber = BuildNextRevisionCode(existing.PONumber!, newRevisionNo);
            dto.AmendmentReason = dto.AmendmentReason?.Trim();

            // Normalize document filenames
            if (dto.Documents is { Count: > 0 })
            {
                var baseDir = "PoDocument";
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                foreach (var doc in dto.Documents.Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName)))
                {
                    var oldPath = Path.Combine(uploadDir, doc.FileName!);
                    if (File.Exists(oldPath))
                    {
                        var finalName = $"{dto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
                        var newPath = Path.Combine(uploadDir, finalName);
                        if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Move(oldPath, newPath, overwrite: true);
                            doc.FileName = finalName;
                        }
                    }

                    if (doc.UploadedDate == default)
                        doc.UploadedDate = now;
                }
            }

            // Build REVISED (entity) just for audit fields here; the repo will map scalars & children from DTO
            var revisedForAudit = new PurchaseOrderHeader
            {
                Id = 0,
                UnitId = existing.UnitId,
                OldPOId = existing.OldPOId ?? existing.Id,
                RevisionNo = newRevisionNo,
                AmendmentReason = dto.AmendmentReason,
                POMethodId = existing.POMethodId,
                CreatedBy = _ip.GetUserId(),
                CreatedByName = _ip.GetUserName(),
                CreatedIP = _ip.GetSystemIPAddress(),
                CreatedDate = now,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

            // Pre-compute approval workflow values (don't depend on amend result)
            var unitId = _ip.GetUnitId()
                ?? throw new InvalidOperationException("UnitId is not available for the current user.");

            var poCategory = await _misc.GetByIdAsync(dto.POCategoryId);
            var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
            var approvalTypeName = isEmergency
                ? MiscEnumEntity.TransactionTypeEPO
                : MiscEnumEntity.TransactionTypeIPO;

            var approvalTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                approvalTypeName, MiscEnumEntity.ModulePurchase, unitId);

            // ════════════════════════════════════════════════════════════════════════
            // ATOMIC TRANSACTION: Amend PO + Outbox Events
            // ════════════════════════════════════════════════════════════════════════

            var strategy = _cmd.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var (transaction, dbConn, dbTx) = await _cmd.BeginTransactionWithConnectionAsync(ct);
                await using var _ = transaction;

                try
                {
                    var newId = await _cmd.AmendWithoutTransactionAsync(existing, dto, revisedForAudit, ct);

                    // ── Approval workflow (outbox — same transaction) ──────────────────
                    var correlationId = Guid.NewGuid();
                    var workFlowEntity = await _cmd.GetByIdImportPOWorkFlowAsync(newId);
                    var reversePayload = new CreateImportPOReverseDto
                    {
                        Header = workFlowEntity,
                        Lines = null
                    };

                    var workflowCommand = new CreateApprovalRequestCommand
                    {
                        CorrelationId = correlationId,
                        ModuleTypeName = MiscEnumEntity.POLocal,
                        ModuleTransactionId = newId,
                        Payload = JsonSerializer.Serialize(reversePayload),
                        TransactionTypeId = approvalTypeId
                    };

                    await _outboxEventPublisher.ScheduleWithoutSaveAsync(workflowCommand, correlationId, ct);

                    await _cmd.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return newId;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
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
