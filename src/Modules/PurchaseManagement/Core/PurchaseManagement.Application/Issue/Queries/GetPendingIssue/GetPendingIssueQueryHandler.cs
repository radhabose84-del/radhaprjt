using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IInvetoryManagement;
using Contracts.Interfaces.External.IUser;
using Contracts.Interfaces.External.IWarehouse;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;

namespace PurchaseManagement.Application.Issue.Queries.GetPendingIssue
{
    public class GetPendingIssueQueryHandler : IRequestHandler<GetPendingIssueQuery, List<GetPendingIssueDto>>
    {
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentAllLookup;
        private readonly IUOMLookup _uOMLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IRackLookup _rackLookup;
        private readonly IBinLookup _binLookup;
        private readonly IMiscMasterLookup _miscMasterLookup;
        private readonly IItemPurchaseToleranceLookup _iItemPurchaseToleranceLookup;


        public GetPendingIssueQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator, 
        IUnitLookup unitLookup, IDepartmentLookup departmentAllLookup, 
        IUOMLookup uOMLookup, IWarehouseLookup warehouseLookup, IRackLookup rackLookup, 
        IBinLookup binLookup, IMiscMasterLookup miscMasterLookup, IItemPurchaseToleranceLookup iItemPurchaseToleranceLookup)
        {
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentAllLookup = departmentAllLookup;
            _uOMLookup = uOMLookup;
            _warehouseLookup = warehouseLookup;
            _rackLookup = rackLookup;
            _binLookup = binLookup;
            _miscMasterLookup = miscMasterLookup;
            _iItemPurchaseToleranceLookup = iItemPurchaseToleranceLookup;
        }

    public async Task<List<GetPendingIssueDto>> Handle(GetPendingIssueQuery request, CancellationToken cancellationToken)
{
    // 1️⃣ Fetch pending issue data
    var result = await _iissueQueryCommandRepository.GetPendingIssuesAsync(request.MrsNo);
    var pending = _mapper.Map<List<GetPendingIssueDto>>(result);

    if (pending == null || !pending.Any())
        return new List<GetPendingIssueDto>();

    // 2️⃣ Gather distinct IDs
    var itemIds = result.SelectMany(r => r.PendingIssueDetails.Select(d => d.ItemId)).Distinct().ToList();
    var warehouseIds = result.Select(r => r.SubStoresWarehouseId)
                             .Concat(result.SelectMany(r => r.PendingIssueDetails.Select(d => d.WarehouseStockId)))
                             .Where(id => id > 0)
                             .Distinct()
                             .ToList();

    var uomIds = result.SelectMany(r => r.PendingIssueDetails.Select(d => d.UomId))
                       .Distinct()
                       .ToList();

    // 3️⃣ Fire parallel gRPC calls
    var unitTask = _unitLookup.GetAllUnitAsync();
    var departmentTask = _departmentAllLookup.GetAllDepartmentAsync();
    var uomTask = _uOMLookup.GetAllAsync();
    var warehouseTask = _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken);
    var toleranceTask = _iItemPurchaseToleranceLookup.GetByIdsAsync(itemIds, cancellationToken);

    // 🔹 Fetch StorageType master for Bin/Rack differentiation
    var storageTypesTask = _miscMasterLookup.GetMiscMasterByIdAsync("StorageType");

    // Wait for basics first
    await Task.WhenAll(unitTask, departmentTask, uomTask, storageTypesTask, toleranceTask, warehouseTask);
    var warehouseResults = await warehouseTask;

    // 4️⃣ Prepare lookup dictionaries
    var unitLookup = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
    var departmentLookup = (await departmentTask).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
    var uomLookup = (await uomTask).ToDictionary(u => u.Id, u => u.UOMName);
    var warehouseLookup = warehouseResults
        .Where(w => w != null)
        .GroupBy(w => w.Id)
        .ToDictionary(g => g.Key, g => g.First().WarehouseName);

    var storageTypes = await storageTypesTask;

    var binTypeIds = storageTypes
        .Where(x => string.Equals(x.Description, "Bin", StringComparison.OrdinalIgnoreCase))
        .Select(x => x.Id)
        .ToHashSet();

    var rackTypeIds = storageTypes
        .Where(x => string.Equals(x.Description, "Rack", StringComparison.OrdinalIgnoreCase))
        .Select(x => x.Id)
        .ToHashSet();
        
      // 7️⃣ Build lookup dictionaries
    var tolMap = toleranceTask.Result
                .GroupBy(t => t.ItemId)
                .ToDictionary(g => g.Key, g => g.First());

    // 5️⃣ Process and enrich each record
            foreach (var dto in pending)
            {
                if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
                    dto.UnitName = unitName;

                if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.DepartmentName = deptName;

                if (departmentLookup.TryGetValue(dto.SubDepartmentId, out var subDeptName))
                    dto.SubDepartmentName = subDeptName;

                if (warehouseLookup.TryGetValue(dto.SubStoresWarehouseId, out var subWarehouseName))
                    dto.SubStoresWarehouseName = subWarehouseName;

                foreach (var detail in dto.PendingIssueDetails)
                {
                    if (uomLookup.TryGetValue(detail.UomId, out var uomName))
                        detail.UomName = uomName;

                    if (warehouseLookup.TryGetValue(detail.WarehouseStockId, out var warehouseName))
                        detail.WarehouseStockName = warehouseName;

                    if (tolMap.TryGetValue(detail.ItemId, out var tol))
                    {
                    detail.ItemName = tol.ItemName;
                    detail.ItemCode = tol.ItemCode;
                    }

                    // 🔹 Fetch bin-wise stock
                    var itemIdsForQuery = new List<int> { detail.ItemId };
                    var binStockList = await _iissueQueryCommandRepository
                        .GetMainStoresStockBinWise(itemIdsForQuery, detail.WarehouseStockId);

                    // 🔹 Identify Bin and Rack targets
                    var binIds = binStockList
                        .Where(b => binTypeIds.Contains(b.StorageTypeId))
                        .Select(b => b.TargetId)
                        .Distinct()
                        .ToList();

                    var rackIds = binStockList
                        .Where(b => rackTypeIds.Contains(b.StorageTypeId))
                        .Select(b => b.TargetId)
                        .Distinct()
                        .ToList();

                    // 🔹 Fire Bin/Rack gRPC lookups
                    var binTask = _binLookup.GetByIdsAsync(binIds, cancellationToken);
                    var rackTask = _rackLookup.GetByIdsAsync(rackIds, cancellationToken);

                    await Task.WhenAll(binTask, rackTask);

                    var binById = (await binTask)
                        .Where(x => x != null)
                        .GroupBy(x => x.Id)
                        .ToDictionary(g => g.Key, g => g.First());
                    var rackById = (await rackTask)
                        .Where(x => x != null)
                        .GroupBy(x => x.Id)
                        .ToDictionary(g => g.Key, g => g.First());

                    // 🔹 Map enriched stock bins
                    detail.PendingStock = binStockList
                        .Select(s =>
                        {
                            string targetName = "NA", targetCode = "NA";

                            if (binTypeIds.Contains(s.StorageTypeId) && binById.TryGetValue(s.TargetId, out var bin))
                            {
                                targetName = bin.BinName ?? "NA";
                                targetCode = bin.BinCode ?? "NA";
                            }
                            else if (rackTypeIds.Contains(s.StorageTypeId) && rackById.TryGetValue(s.TargetId, out var rack))
                            {
                                targetName = rack.RackName ?? "NA";
                                targetCode = rack.RackCode ?? "NA";
                            }

                            return new GetPendingIssueDto.GetPendingStockBinDto
                            {
                                ItemId = s.ItemId,
                                WarehouseId = s.WarehouseId,
                                StorageTypeId = s.StorageTypeId,
                                StorageTypeName = storageTypes.FirstOrDefault(st => st.Id == s.StorageTypeId)?.Description ?? "NA",
                                TargetId = s.TargetId,
                                TargetCode = targetCode,
                                TargetName = targetName,
                                UomId = s.UomId,
                                UomName = uomLookup.TryGetValue(s.UomId, out var binUom) ? binUom : s.UomName,
                                CurrentStockQty = s.CurrentStockQty,
                                CurrentStockValue = s.CurrentStockValue,
                                AvgRate = s.AvgRate
                            };
                        })
                        .ToList();
                }
            }

    // 6️⃣ Publish domain event
    var domainEvent = new AuditLogsDomainEvent(
        actionDetail: "GetAll",
        actionCode: "GetPendingIssueQuery",
        actionName: pending.Count.ToString(),
        details: "Pending Issue details fetched including Bin/Rack stock info.",
        module: "IssueEntry"
    );

    await _mediator.Publish(domainEvent, cancellationToken);

    return pending;
}


    }
}
