// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWarehouse;
// using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
// using InventoryManagement.Application.Common.Interfaces.IReports.IStockReport;
// using InventoryManagement.Domain.Events;
// using MediatR;

// namespace InventoryManagement.Application.Reports.SubStoresStock
// {
//     public class GetSubStockReportSummaryQueryHandler : IRequestHandler<GetSubStockReportSummaryQuery, List<SubStockSummaryDto>>
//     {
//         private readonly IStockReportQueryRepository _istockReportQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IWarehouseGrpcClient _warehouseGrpcClient;
//         private readonly IRackGrpcClient _rackClient;
//         private readonly IBinGrpcClient _binClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

//         public GetSubStockReportSummaryQueryHandler(IStockReportQueryRepository istockReportQueryRepository, IMapper mapper, IMediator mediator
//             , IWarehouseGrpcClient warehouseGrpcClient
//             , IRackGrpcClient rackClient, IBinGrpcClient binClient, IDepartmentAllGrpcClient departmentAllGrpcClient, IMiscMasterQueryRepository miscMasterQueryRepository)
//         {
//             _istockReportQueryRepository = istockReportQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _warehouseGrpcClient = warehouseGrpcClient;
//             _rackClient = rackClient;
//             _binClient = binClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//         }
//         public async Task<List<SubStockSummaryDto>> Handle(GetSubStockReportSummaryQuery request, CancellationToken cancellationToken)
//         {
//              // 1️⃣ Get stock summary from repository
//             var stockList = await _istockReportQueryRepository.GetSubStoresStockSummaryAsync(
//                 request.ItemId,
//                 request.DepartmentId,
//                 request.WarehouseId,
//                 request.StorageTypeId,
//                 request.TargetId
//             );

//             var stockReport = _mapper.Map<List<SubStockSummaryDto>>(stockList);

//               // 1.1️⃣ Apply additional filtering if any request parameter is specified
//                 stockReport = stockReport
//                     .Where(s =>
//                         (!request.ItemId.HasValue || s.ItemId == request.ItemId.Value) &&
//                         (!request.DepartmentId.HasValue || s.DepartmentId == request.DepartmentId.Value) &&
//                         (!request.WarehouseId.HasValue || s.WarehouseId == request.WarehouseId.Value) &&
//                         (!request.StorageTypeId.HasValue || s.StorageTypeId == request.StorageTypeId.Value) &&
//                         (!request.TargetId.HasValue || s.TargetId == request.TargetId.Value)
//                     )
//                     .ToList();

//             // 2️⃣ Collect distinct IDs for parallel lookups
//             var itemIds = stockReport
//                 .Where(s => s.ItemId > 0)
//                 .Select(s => s.ItemId)
//                 .Distinct()
//                 .ToList();

//             var warehouseIds = stockReport
//                 .Where(s => s.WarehouseId > 0)
//                 .Select(s => s.WarehouseId)
//                 .Distinct()
//                 .ToList();

//             var uomIds = stockReport
//                 .Where(s => s.UomId > 0)
//                 .Select(s => s.UomId)
//                 .Distinct()
//                 .ToList();

//             var departmentIds = stockReport
//                 .Where(s => s.DepartmentId > 0)
//                 .Select(s => s.DepartmentId)
//                 .Distinct()
//                 .ToList();

            

//             // 3️⃣ Fetch all storage types for identifying Bin and Rack
//             var storageTypes = await _miscMasterQueryRepository.GetMiscMaster(
//                                 searchPattern: string.Empty,
//                                 miscTypeCode: null,
//                                 miscTypeDesc: "StorageType"
//                             );

//             var binTypeIds = storageTypes
//                 .Where(x => string.Equals(x.Description, "Bin", StringComparison.OrdinalIgnoreCase))
//                 .Select(x => x.Id)
//                 .ToHashSet();

//             var rackTypeIds = storageTypes
//                 .Where(x => string.Equals(x.Description, "Rack", StringComparison.OrdinalIgnoreCase))
//                 .Select(x => x.Id)
//                 .ToHashSet();

//             // 4️⃣ Identify Bin and Rack target IDs
//             var binIds = stockReport
//                 .Where(s => binTypeIds.Contains(s.StorageTypeId))
//                 .Select(s => s.TargetId)
//                 .Where(id => id > 0)
//                 .Distinct()
//                 .ToList();

//             var rackIds = stockReport
//                 .Where(s => rackTypeIds.Contains(s.StorageTypeId))
//                 .Select(s => s.TargetId)
//                 .Where(id => id > 0)
//                 .Distinct()
//                 .ToList();

//             // 5️⃣ Fire all gRPC / lookup tasks in parallel
//             //var toleranceTask = _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIds, cancellationToken);
//             var warehouseTasks = warehouseIds.Select(id => _warehouseGrpcClient.GetByIdAsync(id, cancellationToken)).ToList();
//             var binTasks = binIds.Select(id => _binClient.GetByIdAsync(id)).ToList();
//             var rackTasks = rackIds.Select(id => _rackClient.GetByIdAsync(id)).ToList();
//             //var uomTasks = uomIds.Select(id => _uomGrpcClient.GetByIdAsync(id,cancellationToken)).ToList();
//             var departmentTask = _departmentAllGrpcClient.GetDepartmentAllAsync(); // ✅ single call

//             // 6️⃣ Await all tasks together
//             await Task.WhenAll(
//                 Task.WhenAll(warehouseTasks),
//                 Task.WhenAll(binTasks),
//                 Task.WhenAll(rackTasks),
//                 departmentTask
//             );

//             // 7️⃣ Build lookup dictionaries
//             // var tolMap = toleranceTask.Result
//             //     .GroupBy(t => t.ItemId)
//             //     .ToDictionary(g => g.Key, g => g.First());

//             var warehouseById = warehouseTasks
//                 .Select(t => t.Result)
//                 .Where(x => x != null)
//                 .ToDictionary(x => x.Id, x => x.WarehouseName);

//             var binById = binTasks
//                 .Select(t => t.Result)
//                 .Where(b => b != null)
//                 .ToDictionary(b => b.Id, b => b);

//             var rackById = rackTasks
//                 .Select(t => t.Result)
//                 .Where(r => r != null)
//                 .ToDictionary(r => r.Id, r => r);

//             // var uomById = uomTasks
//             //     .Select(t => t.Result)
//             //     .Where(u => u != null)
//             //     .ToDictionary(u => u.Id, u => u);
                
//             // ✅ Department lookup
//             var departments = departmentTask.Result;
//             var departmentById = departments
//                 .Where(d => d.DepartmentId > 0)
//                 .ToDictionary(d => d.DepartmentId, d => d);



//             // 8️⃣ Enrich stock report
//             foreach (var stock in stockReport)
//             {
//                 // 🧩 Item details
//                 // if (tolMap.TryGetValue(stock.ItemId, out var tol))
//                 // {
//                 //     stock.ItemName = tol.ItemName;
//                 //     stock.ItemCode = tol.ItemCode;
//                 // }

//                 // 🏭 Warehouse details
//                 if (warehouseById.TryGetValue(stock.WarehouseId, out var whName))
//                 {
//                     stock.WarehouseName = whName;
//                 }
//                 else
//                 {
//                     stock.WarehouseName = "NA";
//                 }

//                 // 🗄️ Storage Type
//                 if (storageTypes.FirstOrDefault(s => s.Id == stock.StorageTypeId) is { Description: var stName })
//                 {
//                     stock.StorageTypeName = stName;
//                 }
//                 else
//                 {
//                     stock.StorageTypeName = "NA";
//                 }

//                 // 📦 Bin / Rack enrichment
//                 if (binTypeIds.Contains(stock.StorageTypeId) && binById.TryGetValue(stock.TargetId, out var bin))
//                 {
//                     stock.TargetName = bin.BinName ?? "NA";
//                     stock.TargetCode = bin.BinCode ?? "NA";
//                 }
//                 else if (rackTypeIds.Contains(stock.StorageTypeId) && rackById.TryGetValue(stock.TargetId, out var rack))
//                 {
//                     stock.TargetName = rack.RackName ?? "NA";
//                     stock.TargetCode = rack.RackCode ?? "NA";
//                 }
//                 else
//                 {
//                     stock.TargetName = "NA";
//                     stock.TargetCode = "NA";
//                 }

//                 // 📏 UOM
//                 // if (uomById.TryGetValue(stock.UomId, out var uom))
//                 // {
//                 //     stock.UomName = uom.UOMName ?? "NA";
//                 // }
//                 // else
//                 // {
//                 //     stock.UomName = "NA";
//                 // }

//                 // 🏢 Department
//                if (departmentById.TryGetValue(stock.DepartmentId, out var department))
//                 {
//                     stock.DepartmentName = department.DepartmentName ?? "NA";
//                 }
//                 else
//                 {
//                     stock.DepartmentName = "NA";
//                 }
//             }

//             // 5️⃣ Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "GetSubStockReportSummaryQuery",
//                 actionName: stockReport.Count.ToString(),
//                 details: $"SubStores Stock details were fetched.",
//                 module: "SubStoresStock"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);

//             return stockReport;
//         }
//     }
// }