using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.POAmendment;
using PurchaseManagement.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Amend;

public sealed class POAmendmentCommandHandler : IRequestHandler<POAmendmentCommand, int>
{
    private readonly IPurchaseOrderCommandRepository _cmd;
    private readonly IPurchaseOrderQueryRepository _qry;
    private readonly IMiscMasterQueryRepository _misc;
    private readonly IMapper _mapper;
    private readonly IIPAddressService _ip;
    private readonly ITimeZoneService _tz;
    private readonly ILogger<POAmendmentCommandHandler> _logger;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IPODocumentQueryRepository _poDocs;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;

    public POAmendmentCommandHandler(
        IPurchaseOrderCommandRepository cmd,
        IPurchaseOrderQueryRepository qry,
        IMiscMasterQueryRepository misc,
        IMapper mapper,
        IIPAddressService ip,
        ITimeZoneService tz,
        ILogger<POAmendmentCommandHandler> logger,
        IOutboxEventPublisher outboxEventPublisher,
        IPODocumentQueryRepository poDocs,
        IDocumentSequenceLookup documentSequenceLookup)
    {
        _cmd = cmd; _qry = qry; _misc = misc; _mapper = mapper;
        _ip = ip; _tz = tz; _logger = logger;
        _outboxEventPublisher = outboxEventPublisher;
        _poDocs = poDocs;
        _documentSequenceLookup = documentSequenceLookup;
    }

    public async Task<int> Handle(POAmendmentCommand request, CancellationToken ct)
    {
        var dto = request.Data ?? throw new ValidationException("Body required.");
        if (dto.Id <= 0) throw new ValidationException("Purchase Order id is required.");

        // Load existing PO and guards
        var existing = await _cmd.GetAggregateAsync(dto.Id, ct)
                       ?? throw new InvalidOperationException($"PO {dto.Id} not found.");

        var approved = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
        if (existing.StatusId != approved.Id)
            throw new InvalidOperationException("PO is not approved. Use Edit API for Pending POs; Amendment is only for Approved POs.");

        if (await _qry.HasAnyGrnAsync(dto.Id, ct))
            throw new InvalidOperationException("GRN exists for this PO. Amendment is not allowed.");

        // Timezone
        var tzId = _tz.GetSystemTimeZone();
        if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
            tzId = "Asia/Kolkata";
        TimeZoneInfo tzi; try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); } catch { tzi = TimeZoneInfo.Local; }
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

        // Compute next PONumber & revision on the DTO itself (repo also ensures uniqueness)
        dto.RevisionNo = existing.RevisionNo + 1;
        dto.PONumber   = BuildNextRevisionCode(existing.PONumber, dto.RevisionNo);
        dto.AmendmentReason = dto.AmendmentReason?.Trim();

        // Normalize document filenames in-place on DTO (delete & recreate policy handled in repo)
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
                    var newPath   = Path.Combine(uploadDir, finalName);
                    if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(oldPath, newPath, overwrite: true);
                        doc.FileName = finalName; // update DTO so repo inserts correct name
                    }
                }

                if (doc.UploadedDate == default)
                    doc.UploadedDate = now;
            }
        }

        // Pre-compute approval workflow values (don't depend on amend result)
        var unitId = _ip.GetUnitId()
            ?? throw new InvalidOperationException("UnitId is not available for the current user.");

        var poCategory = await _misc.GetByIdAsync(dto.POCategoryId);
        var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
        var approvalTypeName = isEmergency
            ? MiscEnumEntity.TransactionTypeEPO
            : MiscEnumEntity.TransactionTypeLPO;

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
                var newId = await _cmd.AmendWithoutTransactionAsync(existing, dto, ct);

                // ── Approval workflow (outbox — same transaction) ──────────────────
                var workFlowEntity = await _cmd.GetByIdPOLocalWorkFlowAsync(newId);
                var reversePayload = new CreatePOLocalReverseDto
                {
                    Header = workFlowEntity,
                    Lines = null
                };

                var correlationId = Guid.NewGuid();
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
