using System.ComponentModel.DataAnnotations;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.POAmendment;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Domain.Common;
using MediatR;
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
    //private readonly IEventPublisher _events;
    private readonly IPODocumentQueryRepository _poDocs;

    public POAmendmentCommandHandler(
        IPurchaseOrderCommandRepository cmd,
        IPurchaseOrderQueryRepository qry,
        IMiscMasterQueryRepository misc,
        IMapper mapper,
        IIPAddressService ip,
        ITimeZoneService tz,
        ILogger<POAmendmentCommandHandler> logger,
        //IEventPublisher events,
        IPODocumentQueryRepository poDocs)
    {
        _cmd = cmd; _qry = qry; _misc = misc; _mapper = mapper;
        _ip = ip; _tz = tz; _logger = logger;// _events = events;
         _poDocs = poDocs;
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

        // Call repo with DTO (matches signature): delete old children/docs, soft-close old, insert revised + docs
        var newId = await _cmd.AmendAsync(existing, dto, ct);

        // // Best-effort workflow publish
        // try
        // {
        //     var agg = await _qry.GetByIdAsync(newId, ct);
        //     if (agg is not null)
        //     {
        //         var payload = _mapper.Map<CreatePOLocalReverseDto>(agg);
        //         var evt = new TransactionCreatedEvent
        //         {
        //             CorrelationId = Guid.NewGuid(),
        //             ModuleTypeName = MiscEnumEntity.POLocal,
        //             ModuleTransactionId = newId,
        //             Payload = JsonSerializer.Serialize(payload)
        //         };
        //         await _events.SaveEventAsync(evt);
        //         await _events.PublishPendingEventsAsync();
        //     }
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Workflow publish failed for amended PO={NewId}", newId);
        // }

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
