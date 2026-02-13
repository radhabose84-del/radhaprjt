using AutoMapper;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IIssue;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Issue.Queries.GetPendingIssue
{
    public class GetPendingIssueQueryHandler : IRequestHandler<GetPendingIssueQuery, List<GetPendingIssueDto>>
    {
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IRackLookup _rackLookup;
        private readonly IBinLookup _binLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
       // private readonly IMiscMasterGrpcClient _miscMasterGrpcClient;
        public GetPendingIssueQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator, IUnitLookup unitLookup, IDepartmentLookup departmentLookup, IWarehouseLookup warehouseLookup, IRackLookup rackLookup, IBinLookup binLookup, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
            _warehouseLookup = warehouseLookup;
            _rackLookup = rackLookup;
            _binLookup = binLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            // _miscMasterGrpcClient = miscMasterGrpcClient;

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

                // 3️⃣ Fire parallel lookup calls
                var unitTask = _unitLookup.GetAllUnitAsync();
                var departmentTask = _departmentLookup.GetAllDepartmentAsync();
               // var uomTask = _uOMGrpcClient.GetUOMAsync();
                var warehouseTask = warehouseIds.Any()
                    ? _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken)
                    : Task.FromResult<IReadOnlyList<WarehouseLookupDto>>(Array.Empty<WarehouseLookupDto>());
               // var toleranceTask = _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIds, cancellationToken);

                // 🔹 Fetch StorageType master for Bin/Rack differentiation
                var storageTypes = await _miscMasterQueryRepository.GetMiscMaster(
                                    searchPattern: string.Empty,
                                    miscTypeCode: MiscEnumEntity.StorageType,
                                    miscTypeDesc: null
                                );

                // Wait for lookups to finish
                await Task.WhenAll(unitTask, departmentTask, warehouseTask);

                // 4️⃣ Prepare lookup dictionaries
                var unitLookup = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
                var departmentLookup = (await departmentTask).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
                //var uomLookup = (await uomTask).ToDictionary(u => u.Id, u => u.UOMName);
                var warehouseLookup = (await warehouseTask)
                    .Where(w => w != null)
                    .ToDictionary(w => w.Id, w => w.WarehouseName);

              //  var storageTypes = await storageTypesTask;

                var binTypeIds = storageTypes
                    .Where(x => string.Equals(x.Description, "Bin", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Id)
                    .ToHashSet();

                var rackTypeIds = storageTypes
                    .Where(x => string.Equals(x.Description, "Rack", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Id)
                    .ToHashSet();
                    
                // 7️⃣ Build lookup dictionaries
                // var tolMap = toleranceTask.Result
                //             .GroupBy(t => t.ItemId)
                //             .ToDictionary(g => g.Key, g => g.First());

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

                                if (warehouseLookup.TryGetValue(detail.WarehouseStockId, out var warehouseName))
                                    detail.WarehouseStockName = warehouseName;

                           

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

                                // 🔹 Fire Bin/Rack lookup calls
                                var binLookupTask = binIds.Any()
                                    ? _binLookup.GetByIdsAsync(binIds, cancellationToken)
                                    : Task.FromResult<IReadOnlyList<BinLookupDto>>(Array.Empty<BinLookupDto>());
                                var rackLookupTask = rackIds.Any()
                                    ? _rackLookup.GetByIdsAsync(rackIds, cancellationToken)
                                    : Task.FromResult<IReadOnlyList<RackLookupDto>>(Array.Empty<RackLookupDto>());

                                await Task.WhenAll(binLookupTask, rackLookupTask);

                                var binById = (await binLookupTask)
                                    .Where(x => x != null)
                                    .ToDictionary(x => x.Id, x => x);
                                var rackById = (await rackLookupTask)
                                    .Where(x => x != null)
                                    .ToDictionary(x => x.Id, x => x);

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
                                            UomName = s.UomName,
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
