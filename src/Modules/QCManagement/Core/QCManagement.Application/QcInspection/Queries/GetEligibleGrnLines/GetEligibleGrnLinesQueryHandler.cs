using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.GetEligibleGrnLines
{
    public class GetEligibleGrnLinesQueryHandler : IRequestHandler<GetEligibleGrnLinesQuery, IReadOnlyList<EligibleGrnLineDto>>
    {
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IGrnLookup _grnLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ISupplierLookup _supplierLookup;

        public GetEligibleGrnLinesQueryHandler(
            IQcInspectionQueryRepository queryRepository,
            IGrnLookup grnLookup,
            IItemLookup itemLookup,
            ISupplierLookup supplierLookup)
        {
            _queryRepository = queryRepository;
            _grnLookup = grnLookup;
            _itemLookup = itemLookup;
            _supplierLookup = supplierLookup;
        }

        public async Task<IReadOnlyList<EligibleGrnLineDto>> Handle(GetEligibleGrnLinesQuery request, CancellationToken cancellationToken)
        {
            var lines = await _grnLookup.GetGrnLinesAsync(request.SupplierId, request.FromDate, request.ToDate, cancellationToken);
            if (lines.Count == 0)
                return new List<EligibleGrnLineDto>();

            // Exclude GRN lines that already have an inspection
            var inspected = (await _queryRepository.GetInspectedGrnDetailIdsAsync(lines.Select(l => l.GrnDetailId))).ToHashSet();
            var candidates = lines.Where(l => !inspected.Contains(l.GrnDetailId)).ToList();
            if (candidates.Count == 0)
                return new List<EligibleGrnLineDto>();

            // Keep only items whose master flags QC as required
            var itemIds = candidates.Select(l => l.ItemId).Distinct().ToList();
            var itemDict = (await _itemLookup.GetByIdsAsync(itemIds, cancellationToken)).ToDictionary(i => i.Id);
            var eligible = candidates
                .Where(l => itemDict.TryGetValue(l.ItemId, out var it) && it.InspectionRequired)
                .ToList();
            if (eligible.Count == 0)
                return new List<EligibleGrnLineDto>();

            // Resolve supplier names (few distinct suppliers per page)
            var supplierNames = new Dictionary<int, string?>();
            foreach (var sid in eligible.Select(e => e.SupplierId).Distinct())
            {
                var s = await _supplierLookup.GetActiveSupplierByIdAsync(sid, cancellationToken);
                supplierNames[sid] = s?.VendorName;
            }

            return eligible.Select(l =>
            {
                itemDict.TryGetValue(l.ItemId, out var it);
                return new EligibleGrnLineDto
                {
                    GrnHeaderId = l.GrnHeaderId,
                    GrnDetailId = l.GrnDetailId,
                    GrnNo = l.GrnNo,
                    GrnDate = l.GrnDate,
                    SupplierId = l.SupplierId,
                    SupplierName = supplierNames.TryGetValue(l.SupplierId, out var sn) ? sn : null,
                    ItemId = l.ItemId,
                    ItemCode = it?.ItemCode,
                    ItemName = it?.ItemName,
                    BatchNumber = l.BatchNumber,
                    ReceivedQuantity = l.ReceivedQuantity,
                    ReceivedUomId = l.ReceivedUomId
                };
            }).ToList();
        }
    }
}
