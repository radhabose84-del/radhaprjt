using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.IReports.IStockReport;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Reports.StockReportDivisionwise
{
    public class GetStockReportDivsionwiseSummaryQueryHandler : IRequestHandler<GetStockReportDivsionwiseSummaryQuery, List<StockSummaryDivsionwiseDto>>
    {
        private readonly IStockReportQueryRepository _istockReportQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IRackLookup _rackLookup;
        private readonly IBinLookup _binLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IUnitLookup _unitLookup;
       
        public GetStockReportDivsionwiseSummaryQueryHandler(IStockReportQueryRepository istockReportQueryRepository, IMapper mapper, IMediator mediator,
            IWarehouseLookup warehouseLookup,
            IRackLookup rackLookup, IBinLookup binLookup, IMiscMasterQueryRepository miscMasterQueryRepository, IUnitLookup unitLookup)
        {
            _istockReportQueryRepository = istockReportQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _warehouseLookup = warehouseLookup;
            _rackLookup = rackLookup;
            _binLookup = binLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _unitLookup = unitLookup;
         
        }

        public async Task<List<StockSummaryDivsionwiseDto>> Handle(GetStockReportDivsionwiseSummaryQuery request, CancellationToken cancellationToken)
        {
            // 1️⃣ Get stock summary from repository
            var stockList = await _istockReportQueryRepository.GetStockReportDivisionSummaryAsync(request.UnitIds??new List<int>(),
                request.ItemId,
                request.WarehouseId,
                request.StorageTypeId,
                request.TargetId
            );

            var stockReport = _mapper.Map<List<StockSummaryDivsionwiseDto>>(stockList);

              // 1.1️⃣ Apply additional filtering if any request parameter is specified
                stockReport = stockReport
                    .Where(s =>
                        (!request.ItemId.HasValue || s.ItemId == request.ItemId.Value) &&
                        (!request.WarehouseId.HasValue || s.WarehouseId == request.WarehouseId.Value) &&
                        (!request.StorageTypeId.HasValue || s.StorageTypeId == request.StorageTypeId.Value) &&
                        (!request.TargetId.HasValue || s.TargetId == request.TargetId.Value)
                    )
                    .ToList();

            // 2️⃣ Collect distinct IDs for parallel lookups
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

            // 3️⃣ Fetch all storage types for identifying Bin and Rack
            var storageTypes = await _miscMasterQueryRepository.GetMiscMaster(
                                searchPattern: string.Empty,
                                miscTypeCode: null,
                                miscTypeDesc: "StorageType"
);

            var binTypeIds = storageTypes
                .Where(x => string.Equals(x.Description, "Bin", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .ToHashSet();

            var rackTypeIds = storageTypes
                .Where(x => string.Equals(x.Description, "Rack", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .ToHashSet();

            // 4️⃣ Identify Bin and Rack target IDs
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

            // 5️⃣ Fire all gRPC / lookup tasks in parallel            
            var warehouseTask = warehouseIds.Any()
                ? _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken)
                : Task.FromResult<IReadOnlyList<WarehouseLookupDto>>(Array.Empty<WarehouseLookupDto>());

            var binTask = binIds.Any()
                ? _binLookup.GetByIdsAsync(binIds, cancellationToken)
                : Task.FromResult<IReadOnlyList<BinLookupDto>>(Array.Empty<BinLookupDto>());

            var rackTask = rackIds.Any()
                ? _rackLookup.GetByIdsAsync(rackIds, cancellationToken)
                : Task.FromResult<IReadOnlyList<RackLookupDto>>(Array.Empty<RackLookupDto>());

            var unitTask = _unitLookup.GetAllUnitAsync();

                      

            // 6️⃣ Await all tasks together
            await Task.WhenAll(warehouseTask, binTask, rackTask, unitTask);
            var unitLookup = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);


            // // 7️⃣ Build lookup dictionaries
            // var tolMap = toleranceTask.Result
            //     .GroupBy(t => t.ItemId)
            //     .ToDictionary(g => g.Key, g => g.First());

            var warehouseById = (await warehouseTask)
                .Where(x => x != null)
                .ToDictionary(x => x.Id, x => x.WarehouseName);

            var binById = (await binTask)
                .Where(b => b != null)
                .ToDictionary(b => b.Id, b => b);

            var rackById = (await rackTask)
                .Where(r => r != null)
                .ToDictionary(r => r.Id, r => r);

            // var uomById = uomTasks
            //     .Select(t => t.Result)
            //     .Where(u => u != null)
            //     .ToDictionary(u => u.Id, u => u);

            // 8️⃣ Enrich stock report
            foreach (var stock in stockReport)
            {
                // 🧩 Item details
                // if (tolMap.TryGetValue(stock.ItemId, out var tol))
                // {
                //     stock.ItemName = tol.ItemName;
                //     stock.ItemCode = tol.ItemCode;
                // }

                // 🏭 Warehouse details
                if (warehouseById.TryGetValue(stock.WarehouseId, out var whName))
                {
                    stock.WarehouseName = whName;
                }
                else
                {
                    stock.WarehouseName = "NA";
                }

                

                // 🗄️ Storage Type
                if (storageTypes.FirstOrDefault(s => s.Id == stock.StorageTypeId) is { Description: var stName })
                {
                    stock.StorageTypeName = stName;
                }
                else
                {
                    stock.StorageTypeName = "NA";
                }

                // 📦 Bin / Rack enrichment
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

                // 📦 Unit
                if (unitLookup.TryGetValue(stock.UnitId, out var unitName))
                {
                    stock.UnitName = unitName;
                }
                else
                {
                    stock.UnitName = "NA";
                }
            }

            // 5️⃣ Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetStockReportDivsionwiseSummaryQuery",
                actionName: stockReport.Count.ToString(),
                details: $"Stock details were fetched.",
                module: "StockReport Divisionwise Summary"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return stockReport;
        }
    }
}
