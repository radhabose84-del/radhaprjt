#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Inventory;
using Contracts.Interfaces.External.IInvetoryManagement;
using Contracts.Interfaces.External.IUser;
using Contracts.Interfaces.External.IWarehouse;
using Contracts.Interfaces.External.IWorkflow;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById
{
    public class GetPendingIssueReturnByIdQueryHandler : IRequestHandler<GetPendingIssueReturnByIdQuery, PendingIssueReturnByIdDto>
    {
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _usersAllLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUOMLookup _uOMLookup;
        private readonly IDepartmentLookup _departmentAllLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IItemPurchaseToleranceLookup _iItemPurchaseToleranceLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IPutawayRuleLookup _putawayRuleLookup;
        private readonly IRackLookup _rackLookup;
        private readonly IBinLookup _binLookup;

        public GetPendingIssueReturnByIdQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMediator mediator, IMapper mapper, 
        IWorkflowLookup workflowLookup, IUserLookup usersAllLookup, IIPAddressService ipAddressService,
        IUOMLookup uOMLookup, IDepartmentLookup departmentAllLookup, IUnitLookup unitLookup, 
        IItemPurchaseToleranceLookup iItemPurchaseToleranceLookup, IWarehouseLookup warehouseLookup, IPutawayRuleLookup putawayRuleLookup, 
        IBinLookup binLookup, IRackLookup rackLookup
        )
        {
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
            _workflowLookup = workflowLookup;
            _usersAllLookup = usersAllLookup;
            _ipAddressService = ipAddressService;
            _uOMLookup = uOMLookup;
            _departmentAllLookup = departmentAllLookup;
            _unitLookup = unitLookup;
            _iItemPurchaseToleranceLookup = iItemPurchaseToleranceLookup;
            _warehouseLookup = warehouseLookup;
            _putawayRuleLookup = putawayRuleLookup;
            _binLookup = binLookup;
            _rackLookup = rackLookup;
        }

    public async Task<PendingIssueReturnByIdDto> Handle(GetPendingIssueReturnByIdQuery request, CancellationToken cancellationToken)
    {
        // 1️⃣ Get IssueReturn by id
        var result = await _iissueQueryCommandRepository.GetByIdAsync(request.Id);
        var indent = _mapper.Map<PendingIssueReturnByIdDto>(result);

        var issueReturnIds = new List<int> { indent.IssueReturnId };
        var itemIds = indent.PendingIssueReturnDetails
                            .Select(d => d.ItemId)
                            .Distinct()
                            .ToList();

        // 2️⃣ Parallel GRPC calls
        var workflowLineTask = _workflowLookup.GetApprovalRequestLineAsync(
            MiscEnumEntity.IssueReturn, issueReturnIds, _ipAddressService.GetUserId());

        var workflowHeaderTask = _workflowLookup.GetApproverListAsync(
            MiscEnumEntity.IssueReturn, issueReturnIds);

        var uomTask = _uOMLookup.GetAllAsync(cancellationToken);
        var departmentTask = _departmentAllLookup.GetAllDepartmentAsync();
        var unitTask = _unitLookup.GetAllUnitAsync();
        var itemsTask = _iItemPurchaseToleranceLookup.GetByIdsAsync(itemIds, cancellationToken);
        var usersTask = _usersAllLookup.GetAllUserAsync();

        await Task.WhenAll(
            workflowLineTask, workflowHeaderTask,
            uomTask, departmentTask, unitTask, itemsTask, usersTask
        );

        // // 3️⃣ Lookups
        var workflowLineLookup = workflowLineTask.Result
            .ToDictionary(d => d.ModuleLineTransactionId, d => d.ApprovalRequestLineTransactionId);

        var workflowHeaderLookup = workflowHeaderTask.Result
            .ToDictionary(d => d.ModuleTransactionId, d => d);

        var uomLookup = uomTask.Result.ToDictionary(d => d.Id, d => d.UOMName);
        var deptLookup = departmentTask.Result.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
        var unitLookup = unitTask.Result.ToDictionary(d => d.UnitId, d => d.UnitName);
        var itemLookup = itemsTask.Result.ToDictionary(d => d.ItemId, d => d);
        var userLookup = usersTask.Result.ToDictionary(d => d.UserId, d => d.UserName);


        // ================================================
        // 4️⃣ PUTAWAY RULE LOGIC
        // ================================================

        Task<List<PutawayRuleDto>> putawayTask = Task.FromResult(new List<PutawayRuleDto>());

        List<int> warehouseStockIds = indent.PendingIssueReturnDetails
            .Select(d => d.WarehouseStockId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (warehouseStockIds.Count == 0)
            warehouseStockIds.Add(0);

        if (indent.RequestCategoryName == MiscEnumEntity.Consumption)
        {
            putawayTask = _putawayRuleLookup.GetPutAwayRuleDetailsByWarehouseAsync(
                itemIds, warehouseStockIds, cancellationToken);
        }
        else if (indent.RequestCategoryName == MiscEnumEntity.SubStores)
        {
            var effectiveWarehouseIds = new List<int>();

            foreach (var stockId in warehouseStockIds)
            {
                var stockWarehouse = (await _warehouseLookup.GetByIdsAsync(new[] { stockId }, cancellationToken)).FirstOrDefault();

                if (stockWarehouse != null && stockWarehouse.ParentWarehouseId.HasValue && stockWarehouse.ParentWarehouseId.Value > 0)
                    effectiveWarehouseIds.Add(stockWarehouse.ParentWarehouseId.Value);
                else
                    effectiveWarehouseIds.Add(stockId);
            }

            effectiveWarehouseIds = effectiveWarehouseIds.Distinct().ToList();
            if (effectiveWarehouseIds.Count == 0)
                effectiveWarehouseIds.Add(0);

            putawayTask = _putawayRuleLookup.GetPutAwayRuleDetailsByWarehouseAsync(
                itemIds, effectiveWarehouseIds, cancellationToken);
        }

        var putawayRules = await putawayTask;

        var putawayLookup = putawayRules
            .GroupBy(x => x.ItemId)
            .ToDictionary(g => g.Key, g => g.ToList());


        // ================================================
        // 5️⃣ FETCH WAREHOUSE DETAILS FOR TARGET DISPLAY
        // ================================================
        var warehouseIds = putawayRules
            .Where(r => r.WarehouseId.HasValue && r.WarehouseId > 0)
            .Select(r => r.WarehouseId!.Value)
            .Distinct()
            .ToList();

        var warehouseResults = warehouseIds.Any()
            ? await _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken)
            : Array.Empty<Contracts.Dtos.Lookups.Warehouse.WarehouseLookupDto>();

        var warehouseLookup = warehouseResults
            .Where(w => w != null)
            .GroupBy(w => w.Id)
            .ToDictionary(g => g.Key, g => g.First());


        // ================================================
        // 6️⃣ BIN & RACK lookups
        // ================================================

        var binTargetIds = putawayRules
            .Where(r => r.StorageTypeName!.ToLower() == "bin")
            .Select(r => r.TargetId)
            .Distinct()
            .ToList();

        var rackTargetIds = putawayRules
            .Where(r => r.StorageTypeName!.ToLower() == "rack")
            .Select(r => r.TargetId)
            .Distinct()
            .ToList();

        var binTask = binTargetIds.Any()
            ? _binLookup.GetByIdsAsync(binTargetIds, cancellationToken)
            : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Warehouse.BinLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Warehouse.BinLookupDto>());

        var rackTask = rackTargetIds.Any()
            ? _rackLookup.GetByIdsAsync(rackTargetIds, cancellationToken)
            : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Warehouse.RackLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Warehouse.RackLookupDto>());

        await Task.WhenAll(binTask, rackTask);

        var binLookup = (await binTask)
            .Where(b => b != null)
            .GroupBy(b => b.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var rackLookup = (await rackTask)
            .Where(r => r != null)
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => g.First());


        // ================================================
        // 7️⃣ Parent (header) mappings
        // ================================================
        if (deptLookup.TryGetValue(indent.DepartmentId, out var deptName))
            indent.DepartmentName = deptName;

        if (unitLookup.TryGetValue(indent.UnitId, out var unitName))
            indent.UnitName = unitName;

        if (workflowHeaderLookup.TryGetValue(indent.IssueReturnId, out var wfHeader))
        {
            indent.ApprovalRequestHeaderId = Convert.ToInt32(wfHeader.ApprovalRequestId);
            indent.ApproverId = Convert.ToInt32(wfHeader.ApproverValue);

            if (userLookup.TryGetValue(indent.ApproverId, out var approverName))
                indent.ApproverName = approverName;
        }


        // ================================================
        // 8️⃣ Child detail mappings + PutawayRuleDisplayDto
        // ================================================
        foreach (var dto in indent.PendingIssueReturnDetails)
        {
            if (workflowLineLookup.TryGetValue(dto.Id, out var wfLine))
                dto.ApprovalRequestLineId = Convert.ToInt32(wfLine);

            if (uomLookup.TryGetValue(dto.UomId, out var uomName))
                dto.UOMName = uomName;

            if (itemLookup.TryGetValue(dto.ItemId, out var item))
            {
                dto.ItemCode = item.ItemCode;
                dto.ItemName = item.ItemName;
            }

            if (deptLookup.TryGetValue(dto.SubStoresDepartmentId, out var subDept))
                dto.SubStoresDepartmentName = subDept;

       // ⭐ Apply Putaway Rules
            if (putawayLookup.TryGetValue(dto.ItemId, out var pList))
            {
                dto.PutawayRules = pList.Select(r =>
                {
                    // default
                    string targetName = r.TargetName;
                    string targetCode = r.TargetCode;

                    if (binLookup.TryGetValue(r.TargetId, out var bin))
                    {
                        targetName = bin.BinName ?? targetName;
                        targetCode = bin.BinCode ?? targetCode;
                    }
                    else if (rackLookup.TryGetValue(r.TargetId, out var rack))
                    {
                        targetName = rack.RackName ?? targetName;
                        targetCode = rack.RackCode ?? targetCode;
                    }

                    return new PutawayRuleDisplayDto
                    {
                        WarehouseId = r.WarehouseId,
                        WarehouseCode = r.WarehouseId.HasValue && warehouseLookup.ContainsKey(r.WarehouseId.Value)
                            ? warehouseLookup[r.WarehouseId.Value].WarehouseCode
                            : null,
                        WarehouseName = r.WarehouseId.HasValue && warehouseLookup.ContainsKey(r.WarehouseId.Value)
                            ? warehouseLookup[r.WarehouseId.Value].WarehouseName
                            : null,

                        StorageTypeId = r.StorageTypeId,
                        StorageTypeName = r.StorageTypeName,
                        TargetId = r.TargetId,
                        TargetCode = targetCode,
                        TargetName = targetName,
                        PriorityId = r.PriorityId,
                        PriorityName = r.PriorityName
                    };
                }).ToList();
            }
        }


        // 9️⃣ Save Audit Log
        var evt = new AuditLogsDomainEvent(
            actionDetail: "GetPendingIssueReturnByIdQuery",
            actionCode: "GetPendingIssueReturnByIdQuery",
            actionName: "GetPendingIssueReturnByIdQuery",
            details: JsonSerializer.Serialize(request),
            module: "IssueReturn");

        await _mediator.Publish(evt, cancellationToken);

        return indent;
    }




    }
}
