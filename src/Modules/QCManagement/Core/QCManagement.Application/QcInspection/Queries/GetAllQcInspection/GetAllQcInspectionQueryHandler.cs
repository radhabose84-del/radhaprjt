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
        private readonly IItemLookup _itemLookup;
        private readonly ISupplierLookup _supplierLookup;
        private readonly IMediator _mediator;

        public GetAllQcInspectionQueryHandler(
            IQcInspectionQueryRepository queryRepository,
            IGrnLookup grnLookup,
            IItemLookup itemLookup,
            ISupplierLookup supplierLookup,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _grnLookup = grnLookup;
            _itemLookup = itemLookup;
            _supplierLookup = supplierLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<QcInspectionListDto>>> Handle(GetAllQcInspectionQuery request, CancellationToken cancellationToken)
        {
            // 1. All generated GRN lines, then keep only QC-required items
            var grnLines = await _grnLookup.GetGrnLinesAsync(null, null, null, cancellationToken);
            var itemIds = grnLines.Select(l => l.ItemId).Distinct().ToList();
            var itemDict = (await _itemLookup.GetByIdsAsync(itemIds, cancellationToken)).ToDictionary(i => i.Id);

            var qcLines = grnLines
                .Where(l => itemDict.TryGetValue(l.ItemId, out var it) && it.InspectionRequired)
                .ToList();

            // 2. Merge with existing inspections (keyed by GRN detail)
            var summaryList = await _queryRepository.GetInspectionSummariesByGrnDetailIdsAsync(qcLines.Select(l => l.GrnDetailId));
            var summaries = summaryList.GroupBy(s => s.GrnDetailId).ToDictionary(g => g.Key, g => g.First());

            // 3. Supplier names (distinct)
            var supplierNames = new Dictionary<int, string?>();
            foreach (var sid in qcLines.Select(l => l.SupplierId).Distinct())
            {
                var s = await _supplierLookup.GetActiveSupplierByIdAsync(sid, cancellationToken);
                supplierNames[sid] = s?.VendorName;
            }

            // 4. Build unified rows (Pending status set here, not in the DTO)
            var rows = qcLines.Select(l =>
            {
                itemDict.TryGetValue(l.ItemId, out var it);
                var row = new QcInspectionListDto
                {
                    GrnHeaderId = l.GrnHeaderId,
                    GrnDetailId = l.GrnDetailId,
                    GrnNo = l.GrnNo,
                    SupplierId = l.SupplierId,
                    SupplierName = supplierNames.TryGetValue(l.SupplierId, out var sn) ? sn : null,
                    ItemId = l.ItemId,
                    ItemName = it?.ItemName,
                    BatchNumber = l.BatchNumber,
                    ReceivedQuantity = l.ReceivedQuantity,
                    QcStatusCode = "PENDING_QC",
                    QcStatusName = "Pending QC"
                };

                if (summaries.TryGetValue(l.GrnDetailId, out var ins))
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
                    (r.GrnNo?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
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
                .ThenByDescending(r => r.GrnDetailId)
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
