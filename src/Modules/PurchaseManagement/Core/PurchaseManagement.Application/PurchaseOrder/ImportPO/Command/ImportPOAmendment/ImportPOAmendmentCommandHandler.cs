// PurchaseManagement.Application/PurchaseOrder/ImportPO/Command/ImportPOAmendment/ImportPOAmendmentCommandHandler.cs
using System.Text.Json;
using AutoMapper;
using Contracts.Events.Workflow;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.PurchaseOrder;
using MediatR;
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
        private readonly IEventPublisher _events;

        public ImportPOAmendmentCommandHandler(
            IImportPOCommandRepository cmd,
            IImportPOQueryRepository qry,
            IMiscMasterQueryRepository misc,
            IMapper mapper,
            IIPAddressService ip,
            ITimeZoneService tz,
            ILogger<ImportPOAmendmentCommandHandler> logger,
            IEventPublisher events)
        {
            _cmd = cmd; _qry = qry; _misc = misc; _mapper = mapper;
            _ip = ip; _tz = tz; _logger = logger; _events = events;
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

            // → REPO will: delete old import children/terms/docs, soft-close existing,
            // insert revised root (scalars from dto + revisedForAudit), then insert docs from dto
            var newId = await _cmd.AmendAsync(existing, dto, revisedForAudit, ct);

            // Best-effort re-approval workflow event
            try
            {
                var agg = await _qry.GetByIdAsync(newId, ct);
                if (agg is not null)
                {
                    var payload = _mapper.Map<CreateImportPOReverseDto>(agg);
                    var evt = new TransactionCreatedEvent
                    {
                        CorrelationId = Guid.NewGuid(),
                        ModuleTypeName = MiscEnumEntity.POLocal,
                        ModuleTransactionId = newId,
                        Payload = JsonSerializer.Serialize(payload)
                    };
                    await _events.SaveEventAsync(evt);
                    await _events.PublishPendingEventsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Workflow publish failed for amended Import PO={NewId}", newId);
            }

            return newId;
        }
    }
}
