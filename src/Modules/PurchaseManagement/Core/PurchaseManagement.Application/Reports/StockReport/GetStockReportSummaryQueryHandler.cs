using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using PurchaseManagement.Application.Common.Interfaces.IReports.IStockReport;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Reports.StockReport
{
    public class GetStockReportSummaryQueryHandler : IRequestHandler<GetStockReportSummaryQuery, List<StockSummaryDto>>
    {

        private readonly IStockReportQueryRepository _istockReportQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IItemPurchaseToleranceLookup _itemPurchaseToleranceLookup;
        private readonly IMiscMasterLookup _miscMasterLookup;
        private readonly IRackLookup _rackLookup;
        private readonly IBinLookup _binLookup;
        private readonly IUOMLookup _uomLookup;

        public GetStockReportSummaryQueryHandler(IStockReportQueryRepository istockReportQueryRepository, IMapper mapper, IMediator mediator
            , IWarehouseLookup warehouseLookup
            , IItemPurchaseToleranceLookup itemPurchaseToleranceLookup
            , IMiscMasterLookup miscMasterLookup
            , IRackLookup rackLookup
            , IBinLookup binLookup
            , IUOMLookup uomLookup)
        {
            _istockReportQueryRepository = istockReportQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _warehouseLookup = warehouseLookup;
            _itemPurchaseToleranceLookup = itemPurchaseToleranceLookup;
            _miscMasterLookup = miscMasterLookup;
            _rackLookup = rackLookup;
            _binLookup = binLookup;
            _uomLookup = uomLookup;
        }

        public async Task<List<StockSummaryDto>> Handle(GetStockReportSummaryQuery request, CancellationToken cancellationToken)
        {
            // 1. Get stock summary from repository
            var stockList = await _istockReportQueryRepository.GetStockSummaryAsync(
                request.ItemId,
                request.WarehouseId,
                request.StorageTypeId,
                request.TargetId
            );

            var stockReport = _mapper.Map<List<StockSummaryDto>>(stockList);

            // 1.1 Apply additional filtering if any request parameter is specified
            stockReport = stockReport
                .Where(s =>
                    (!request.ItemId.HasValue || s.ItemId == request.ItemId.Value) &&
                    (!request.WarehouseId.HasValue || s.WarehouseId == request.WarehouseId.Value) &&
                    (!request.StorageTypeId.HasValue || s.StorageTypeId == request.StorageTypeId.Value) &&
                    (!request.TargetId.HasValue || s.TargetId == request.TargetId.Value)
                )
                .ToList();

            // 2. Collect distinct IDs for parallel lookups
            var itemIds = stockReport
                .Where(s => s.ItemId > 0)
                .Select(s => s.ItemId)
                .Distinct()
                .ToList();

            var warehouseIds = stockReport
                .Where(s => s.WarehouseId > 0)
                .Select(s => s.WarehouseId)
                .Distinct()
                .ToList();

            var uomIds = stockReport
                .Where(s => s.UomId > 0)
                .Select(s => s.UomId)
                .Distinct()
                .ToList();

            // 3. Fetch all storage types for identifying Bin and Rack
            var storageTypes = await _miscMasterLookup.GetMiscMasterByIdAsync("StorageType");

            var binTypeIds = storageTypes
                .Where(x => string.Equals(x.Description, "Bin", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .ToHashSet();

            var rackTypeIds = storageTypes
                .Where(x => string.Equals(x.Description, "Rack", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .ToHashSet();

            // 4. Identify Bin and Rack target IDs
            var binIds = stockReport
                .Where(s => binTypeIds.Contains(s.StorageTypeId))
                .Select(s => s.TargetId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var rackIds = stockReport
                .Where(s => rackTypeIds.Contains(s.StorageTypeId))
                .Select(s => s.TargetId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            // 5. Fire all lookup tasks in parallel
            var toleranceTask = _itemPurchaseToleranceLookup.GetByIdsAsync(itemIds, cancellationToken);
            var warehouseTask = _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken);
            var binTask = _binLookup.GetByIdsAsync(binIds, cancellationToken);
            var rackTask = _rackLookup.GetByIdsAsync(rackIds, cancellationToken);
            var uomTask = _uomLookup.GetByIdsAsync(uomIds, cancellationToken);

            // 6. Await all tasks together
            await Task.WhenAll(toleranceTask, warehouseTask, binTask, rackTask, uomTask);

            // 7. Build lookup dictionaries
            var tolMap = toleranceTask.Result
                .GroupBy(t => t.ItemId)
                .ToDictionary(g => g.Key, g => g.First());

            var warehouseById = warehouseTask.Result
                .Where(x => x != null)
                .ToDictionary(x => x.Id, x => x.WarehouseName);

            var binById = binTask.Result
                .Where(b => b != null)
                .ToDictionary(b => b.Id, b => b);

            var rackById = rackTask.Result
                .Where(r => r != null)
                .ToDictionary(r => r.Id, r => r);

            var uomById = uomTask.Result
                .Where(u => u != null)
                .ToDictionary(u => u.Id, u => u);

            // 8. Enrich stock report
            foreach (var stock in stockReport)
            {
                // Item details
                if (tolMap.TryGetValue(stock.ItemId, out var tol))
                {
                    stock.ItemName = tol.ItemName;
                    stock.ItemCode = tol.ItemCode;
                }

                // Warehouse details
                if (warehouseById.TryGetValue(stock.WarehouseId, out var whName))
                {
                    stock.WarehouseName = whName;
                }
                else
                {
                    stock.WarehouseName = "NA";
                }

                // Storage Type
                if (storageTypes.FirstOrDefault(s => s.Id == stock.StorageTypeId) is { Description: var stName })
                {
                    stock.StorageTypeName = stName;
                }
                else
                {
                    stock.StorageTypeName = "NA";
                }

                // Bin / Rack enrichment
                if (binTypeIds.Contains(stock.StorageTypeId) && binById.TryGetValue(stock.TargetId, out var bin))
                {
                    stock.TargetName = bin.BinName ?? "NA";
                    stock.TargetCode = bin.BinCode ?? "NA";
                }
                else if (rackTypeIds.Contains(stock.StorageTypeId) && rackById.TryGetValue(stock.TargetId, out var rack))
                {
                    stock.TargetName = rack.RackName ?? "NA";
                    stock.TargetCode = rack.RackCode ?? "NA";
                }
                else
                {
                    stock.TargetName = "NA";
                    stock.TargetCode = "NA";
                }

                // UOM
                if (uomById.TryGetValue(stock.UomId, out var uom))
                {
                    stock.UomName = uom.UOMName ?? "NA";
                }
                else
                {
                    stock.UomName = "NA";
                }
            }

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetStockReportSummaryQuery",
                actionName: stockReport.Count.ToString(),
                details: $"Stock details were fetched.",
                module: "StockReport Summary"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return stockReport;
        }

    }
}