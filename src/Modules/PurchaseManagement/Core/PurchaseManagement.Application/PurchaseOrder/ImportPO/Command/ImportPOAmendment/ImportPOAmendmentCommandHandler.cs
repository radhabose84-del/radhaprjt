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

            // Build REVISED (entity) just for audit fields here; the repo will map scalars & children from DTO
            var revisedForAudit = new PurchaseOrderHeader
            {
                Id = 0,
                UnitId = existing.UnitId,
                OldPOId = existing.OldPOId ?? existing.Id,
                RevisionNo = existing.RevisionNo + 1,
                AmendmentReason = dto.AmendmentReason?.Trim(),
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
                    // Use EF Core GetAggregateAsync (same DbContext/transaction) to read
                    // the newly created PO header, including the generated PONumber.
                    var newPo = await _cmd.GetAggregateAsync(newId, ct);
                    if (newPo is not null)
                    {
                        var reversePayload = new CreateImportPOReverseDto
                        {
                            Header = new ImportPOWorkFlowDto
                            {
                                Id = newId,
                                PONumber = newPo.PONumber,
                                VendorId = newPo.VendorId,
                                StatusId = newPo.StatusId,
                                UnitId = unitId
                            },
                            Lines = new List<ImportPOWorkFlowDto>
                            {
                                new ImportPOWorkFlowDto
                                {
                                    Id = newId,
                                    PONumber = newPo.PONumber,
                                    VendorId = newPo.VendorId,
                                    StatusId = newPo.StatusId,
                                    UnitId = unitId
                                }
                            }
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
                    }

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
    }
}
