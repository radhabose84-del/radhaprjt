using Contracts.Common;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QcInspection.Queries.GetAllQcInspection
{
    /// <summary>
    /// Unified grid: every QC-required GRN line. Lines with no inspection are "Pending QC";
    /// inspected lines carry their disposition status (Approved / Conditionally Approved / Hold / Rejected).
    /// </summary>
    public class GetAllQcInspectionQueryHandler : IRequestHandler<GetAllQcInspectionQuery, ApiResponseDTO<List<QcInspectionListDto>>>
    {
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IGrnLookup _grnLookup;
        private readonly IArrivalLookup _arrivalLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ISupplierLookup _supplierLookup;
        private readonly IMediator _mediator;

        public GetAllQcInspectionQueryHandler(
            IQcInspectionQueryRepository queryRepository,
            IGrnLookup grnLookup,
            IArrivalLookup arrivalLookup,
            IItemLookup itemLookup,
            ISupplierLookup supplierLookup,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _grnLookup = grnLookup;
            _arrivalLookup = arrivalLookup;
            _itemLookup = itemLookup;
            _supplierLookup = supplierLookup;
            _mediator = mediator;
        }

        // A source-neutral grid line projected from either a GRN line or an Arrival header.
        private sealed class SourceLine
        {
            public int SourceTypeId { get; init; }
            public string SourceTypeCode { get; init; } = "";
            public int SourceHeaderId { get; init; }
            public int SourceDetailId { get; init; }
            public string? SourceNo { get; init; }
            public int SupplierId { get; init; }
            public int ItemId { get; init; }
            public string? BatchNumber { get; init; }
            public decimal ReceivedQuantity { get; init; }
        }

        public async Task<ApiResponseDTO<List<QcInspectionListDto>>> Handle(GetAllQcInspectionQuery request, CancellationToken cancellationToken)
        {
            // Resolve the source-type discriminator ids (GRN / ARRIVAL).
            var grnTypeId = await _queryRepository.GetSourceTypeIdByCodeAsync("GRN") ?? 0;
            var arrivalTypeId = await _queryRepository.GetSourceTypeIdByCodeAsync("ARRIVAL") ?? 0;

            // 1a. GRN lines → source-neutral (one row per QC-required GRN detail line).
            var grnLines = await _grnLookup.GetGrnLinesAsync(null, null, null, cancellationToken);
            var grnSourceLines = grnLines.Select(l => new SourceLine
            {
                SourceTypeId = grnTypeId,
                SourceTypeCode = "GRN",
                SourceHeaderId = l.GrnHeaderId,
                SourceDetailId = l.GrnDetailId,
                SourceNo = l.GrnNo,
                SupplierId = l.SupplierId,
                ItemId = l.ItemId,
                BatchNumber = l.BatchNumber,
                ReceivedQuantity = l.ReceivedQuantity
            });

            // 1b. Arrival lines → source-neutral (header-level: one row per ArrivalHeader,
            //     using the representative QC-required line for item/qty/batch).
            var arrivalLines = await _arrivalLookup.GetArrivalLinesAsync(null, null, null, cancellationToken);
            var arrivalSourceLines = arrivalLines.Select(l => new SourceLine
            {
                SourceTypeId = arrivalTypeId,
                SourceTypeCode = "ARRIVAL",
                SourceHeaderId = l.ArrivalHeaderId,
                SourceDetailId = l.ArrivalDetailId,
                SourceNo = l.ArrivalNumber,
                SupplierId = l.SupplierId,
                ItemId = l.ItemId,
                BatchNumber = l.BatchNumber,
                ReceivedQuantity = l.ReceivedQuantity
            });

            // 2. Resolve items once, keep only QC-required lines.
            var allLines = grnSourceLines.Concat(arrivalSourceLines).ToList();
            var itemIds = allLines.Select(l => l.ItemId).Distinct().ToList();
            var itemDict = (await _itemLookup.GetByIdsAsync(itemIds, cancellationToken)).ToDictionary(i => i.Id);

            var qcLines = allLines
                .Where(l => itemDict.TryGetValue(l.ItemId, out var it) && it.InspectionRequired)
                .ToList();

            // Arrival is header-level → collapse to one representative line per ArrivalHeader.
            var grnQcLines = qcLines.Where(l => l.SourceTypeCode == "GRN");
            var arrivalQcLines = qcLines
                .Where(l => l.SourceTypeCode == "ARRIVAL")
                .GroupBy(l => l.SourceHeaderId)
                .Select(g => g.OrderBy(l => l.SourceDetailId).First());
            qcLines = grnQcLines.Concat(arrivalQcLines).ToList();

            // 3. Merge with existing inspections (keyed by source type + detail).
            var grnSummaries = await _queryRepository.GetInspectionSummariesBySourceAsync(
                grnTypeId, qcLines.Where(l => l.SourceTypeCode == "GRN").Select(l => l.SourceDetailId));
            var arrivalSummaries = await _queryRepository.GetInspectionSummariesBySourceAsync(
                arrivalTypeId, qcLines.Where(l => l.SourceTypeCode == "ARRIVAL").Select(l => l.SourceDetailId));
            var summaries = grnSummaries.Concat(arrivalSummaries)
                .GroupBy(s => (s.SourceTypeId, s.SourceDetailId))
                .ToDictionary(g => g.Key, g => g.First());

            // 4. Supplier names (distinct)
            var supplierNames = new Dictionary<int, string?>();
            foreach (var sid in qcLines.Select(l => l.SupplierId).Distinct())
            {
                var s = await _supplierLookup.GetActiveSupplierByIdAsync(sid, cancellationToken);
                supplierNames[sid] = s?.VendorName;
            }

            // 5. Build unified rows (Pending status set here, not in the DTO)
            var rows = qcLines.Select(l =>
            {
                itemDict.TryGetValue(l.ItemId, out var it);
                var row = new QcInspectionListDto
                {
                    SourceTypeId = l.SourceTypeId,
                    SourceTypeCode = l.SourceTypeCode,
                    SourceTypeName = l.SourceTypeCode == "ARRIVAL" ? "Arrival" : "GRN",
                    SourceHeaderId = l.SourceHeaderId,
                    SourceDetailId = l.SourceDetailId,
                    SourceNo = l.SourceNo,
                    SupplierId = l.SupplierId,
                    SupplierName = supplierNames.TryGetValue(l.SupplierId, out var sn) ? sn : null,
                    ItemId = l.ItemId,
                    ItemName = it?.ItemName,
                    BatchNumber = l.BatchNumber,
                    ReceivedQuantity = l.ReceivedQuantity,
                    QcStatusCode = "PENDING_QC",
                    QcStatusName = "Pending QC"
                };

                if (summaries.TryGetValue((l.SourceTypeId, l.SourceDetailId), out var ins))
                {
                    row.InspectionId = ins.Id;
                    row.QcInspectionNo = ins.QcInspectionNo;
                    row.AcceptedQuantity = ins.AcceptedQuantity;
                    row.RejectedQuantity = ins.RejectedQuantity;
                    row.QcStatusId = ins.QcStatusId;
                    row.InspectionDate = ins.InspectionDate;
                    row.CreatedDate = ins.CreatedDate;
                    row.CreatedByName = ins.CreatedByName;
                    row.ModifiedDate = ins.ModifiedDate;
                    row.ModifiedByName = ins.ModifiedByName;
                    if (ins.QcStatusId.HasValue)
                    {
                        row.QcStatusCode = ins.QcStatusCode;
                        row.QcStatusName = ins.QcStatusName;
                    }
                }
                return row;
            });

            // 5. Filters (in-memory)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                rows = rows.Where(r =>
                    (r.QcInspectionNo?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.SourceNo?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.SupplierName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.ItemName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.BatchNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            if (request.QcStatusId.HasValue)
                rows = rows.Where(r => r.QcStatusId == request.QcStatusId.Value);
            if (request.InspectionDateFrom.HasValue)
                rows = rows.Where(r => r.InspectionDate.HasValue && r.InspectionDate.Value >= request.InspectionDateFrom.Value);
            if (request.InspectionDateTo.HasValue)
                rows = rows.Where(r => r.InspectionDate.HasValue && r.InspectionDate.Value <= request.InspectionDateTo.Value);

            var ordered = rows
                .OrderByDescending(r => r.InspectionId ?? 0)
                .ThenByDescending(r => r.SourceDetailId)
                .ToList();

            var totalCount = ordered.Count;
            var page = request.PageSize > 0
                ? ordered.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList()
                : ordered;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllQcInspectionQuery",
                actionCode: "Get",
                actionName: page.Count.ToString(),
                details: "QC Inspection grid was fetched.",
                module: "QcInspection"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<QcInspectionListDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = page,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
