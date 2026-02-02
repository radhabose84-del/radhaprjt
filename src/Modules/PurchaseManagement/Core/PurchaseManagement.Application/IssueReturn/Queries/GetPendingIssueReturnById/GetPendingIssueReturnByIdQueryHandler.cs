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

namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById
{
    public class GetPendingIssueReturnByIdQueryHandler : IRequestHandler<GetPendingIssueReturnByIdQuery, PendingIssueReturnByIdDto>
    {
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        // private readonly IWorkflowGrpcClient _workflowGrpcClient;
        // private readonly IUsersAllGrpcClient _usersAllGrpcClient;
        // private readonly IIPAddressService _ipAddressService;
        // private readonly IUOMGrpcClient _uOMGrpcClient;
        // private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
        // private readonly IUnitGrpcClient _unitGrpcClient;
        // private readonly IInventoryGrpcClient _inventoryGrpcClient;
        // private readonly IWarehouseGrpcClient _warehouseGrpcClient;
        // private readonly IPutawayRuleGrpcClient _putawayRuleGrpcClient;
        // private readonly IBinGrpcClient _binGrpcClient;
        // private readonly IRackGrpcClient _rackGrpcClient;
        public GetPendingIssueReturnByIdQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMediator mediator, IMapper mapper
        //, IWorkflowGrpcClient workflowGrpcClient, IUsersAllGrpcClient usersAllGrpcClient, IIPAddressService ipAddressService,
        // IUOMGrpcClient uOMGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient, IUnitGrpcClient unitGrpcClient, IInventoryGrpcClient inventoryGrpcClient, IWarehouseGrpcClient warehouseGrpcClient, IPutawayRuleGrpcClient putawayRuleGrpcClient, IBinGrpcClient binGrpcClient, IRackGrpcClient rackGrpcClient
        )
        {
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
            // _workflowGrpcClient = workflowGrpcClient;
            // _usersAllGrpcClient = usersAllGrpcClient;
            // _ipAddressService = ipAddressService;
            // _uOMGrpcClient = uOMGrpcClient;
            // _departmentAllGrpcClient = departmentAllGrpcClient;
            // _unitGrpcClient = unitGrpcClient;
            // _inventoryGrpcClient = inventoryGrpcClient;
            // _warehouseGrpcClient = warehouseGrpcClient;
            // _putawayRuleGrpcClient = putawayRuleGrpcClient;
            // _binGrpcClient = binGrpcClient;
            // _rackGrpcClient = rackGrpcClient;
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

        // // 2️⃣ Parallel GRPC calls
        // var workflowLineTask = _workflowGrpcClient.GetApprovalRequestLineAsync(
        //     MiscEnumEntity.IssueReturn, issueReturnIds, _ipAddressService.GetUserId());

        // var workflowHeaderTask = _workflowGrpcClient.GetApproverListAsync(
        //     MiscEnumEntity.IssueReturn, issueReturnIds);

        // var uomTask = _uOMGrpcClient.GetUOMAsync();
        // var departmentTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
        // var unitTask = _unitGrpcClient.GetAllUnitAsync();
        // var itemsTask = _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIds, cancellationToken);
        // var usersTask = _usersAllGrpcClient.GetUserAllAsync();

        // await Task.WhenAll(
        //     workflowLineTask, workflowHeaderTask,
        //     uomTask, departmentTask, unitTask, itemsTask, usersTask
        // );

        // // 3️⃣ Lookups
        // var workflowLineLookup = workflowLineTask.Result
        //     .ToDictionary(d => d.ModuleLineTransactionId, d => d.ApprovalRequestLineTransactionId);

        // var workflowHeaderLookup = workflowHeaderTask.Result
        //     .ToDictionary(d => d.ModuleTransactionId, d => d);

        // var uomLookup = uomTask.Result.ToDictionary(d => d.Id, d => d.UOMName);
        // var deptLookup = departmentTask.Result.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
        // var unitLookup = unitTask.Result.ToDictionary(d => d.UnitId, d => d.UnitName);
        // var itemLookup = itemsTask.Result.ToDictionary(d => d.ItemId, d => d);
        // var userLookup = usersTask.Result.ToDictionary(d => d.UserId, d => d.UserName);


        // ================================================
        // 4️⃣ PUTAWAY RULE LOGIC
        // ================================================

        // Task<List<PutawayRuleDto>> putawayTask = Task.FromResult(new List<PutawayRuleDto>());

        // List<int> warehouseStockIds = indent.PendingIssueReturnDetails
        //     .Select(d => d.WarehouseStockId)
        //     .Where(id => id > 0)
        //     .Distinct()
        //     .ToList();

        // if (warehouseStockIds.Count == 0)
        //     warehouseStockIds.Add(0);

        // if (indent.RequestCategoryName == MiscEnumEntity.Consumption)
        // {
        //     putawayTask = _putawayRuleGrpcClient.GetPutAwayRuleDetailsByWarehouseAsync(
        //         itemIds, warehouseStockIds, cancellationToken);
        // }
        // else if (indent.RequestCategoryName == MiscEnumEntity.SubStores)
        // {
        //     var effectiveWarehouseIds = new List<int>();

        //     foreach (var stockId in warehouseStockIds)
        //     {
        //         var stockWarehouse = await _warehouseGrpcClient.GetByIdAsync(stockId, cancellationToken);

        //         if (stockWarehouse != null && stockWarehouse.ParentWarehouseId > 0)
        //             effectiveWarehouseIds.Add(stockWarehouse.ParentWarehouseId);
        //         else
        //             effectiveWarehouseIds.Add(stockId);
        //     }

        //     effectiveWarehouseIds = effectiveWarehouseIds.Distinct().ToList();
        //     if (effectiveWarehouseIds.Count == 0)
        //         effectiveWarehouseIds.Add(0);

        //     putawayTask = _putawayRuleGrpcClient.GetPutAwayRuleDetailsByWarehouseAsync(
        //         itemIds, effectiveWarehouseIds, cancellationToken);
        // }

        // var putawayRules = await putawayTask;

        // var putawayLookup = putawayRules
        //     .GroupBy(x => x.ItemId)
        //     .ToDictionary(g => g.Key, g => g.ToList());


        // ================================================
        // 5️⃣ FETCH WAREHOUSE DETAILS FOR TARGET DISPLAY
        // ================================================
        // var warehouseIds = putawayRules
        //     .Where(r => r.WarehouseId.HasValue && r.WarehouseId > 0)
        //     .Select(r => r.WarehouseId!.Value)
        //     .Distinct()
        //     .ToList();

        // var warehouseTasks = warehouseIds
        //     .Select(id => _warehouseGrpcClient.GetByIdAsync(id, cancellationToken))
        //     .ToList();

        // var warehouseResults = await Task.WhenAll(warehouseTasks);

        // var warehouseLookup = warehouseResults
        //     .Where(w => w != null)
        //     .ToDictionary(w => w.Id, w => w);


        // ================================================
        // 6️⃣ BIN & RACK lookups
        // ================================================

        // var binTargetIds = putawayRules
        //     .Where(r => r.StorageTypeName!.ToLower() == "bin")
        //     .Select(r => r.TargetId)
        //     .Distinct()
        //     .ToList();

        // var rackTargetIds = putawayRules
        //     .Where(r => r.StorageTypeName!.ToLower() == "rack")
        //     .Select(r => r.TargetId)
        //     .Distinct()
        //     .ToList();

        // var binTasks = binTargetIds.Select(async id =>
        // {
        //     try { return await _binGrpcClient.GetByIdAsync(id); }
        //     catch { return null; }
        // }).ToList();

        // var rackTasks = rackTargetIds.Select(async id =>
        // {
        //     try { return await _rackGrpcClient.GetByIdAsync(id); }
        //     catch { return null; }
        // }).ToList();

        // await Task.WhenAll(Task.WhenAll(binTasks), Task.WhenAll(rackTasks));

        // var binLookup = binTasks.Where(t => t.Result != null)
        //     .Select(t => t.Result)
        //     .ToDictionary(b => b.Id, b => b);

        // var rackLookup = rackTasks.Where(t => t.Result != null)
        //     .Select(t => t.Result)
        //     .ToDictionary(r => r.Id, r => r);


        // ================================================
        // 7️⃣ Parent (header) mappings
        // ================================================
        // if (deptLookup.TryGetValue(indent.DepartmentId, out var deptName))
        //     indent.DepartmentName = deptName;

        // if (unitLookup.TryGetValue(indent.UnitId, out var unitName))
        //     indent.UnitName = unitName;

        // if (workflowHeaderLookup.TryGetValue(indent.IssueReturnId, out var wfHeader))
        // {
        //     indent.ApprovalRequestHeaderId = Convert.ToInt32(wfHeader.ApprovalRequestId);
        //     indent.ApproverId = Convert.ToInt32(wfHeader.ApproverValue);

        //     if (userLookup.TryGetValue(indent.ApproverId, out var approverName))
        //         indent.ApproverName = approverName;
        // }


        // ================================================
        // 8️⃣ Child detail mappings + PutawayRuleDisplayDto
        // ================================================
        // foreach (var dto in indent.PendingIssueReturnDetails)
        // {
        //     if (workflowLineLookup.TryGetValue(dto.Id, out var wfLine))
        //         dto.ApprovalRequestLineId = Convert.ToInt32(wfLine);

        //     if (uomLookup.TryGetValue(dto.UomId, out var uomName))
        //         dto.UOMName = uomName;

        //     if (itemLookup.TryGetValue(dto.ItemId, out var item))
        //     {
        //         dto.ItemCode = item.ItemCode;
        //         dto.ItemName = item.ItemName;
        //     }

        //     if (deptLookup.TryGetValue(dto.SubStoresDepartmentId, out var subDept))
        //         dto.SubStoresDepartmentName = subDept;

        //     // ⭐ Apply Putaway Rules
        //     if (putawayLookup.TryGetValue(dto.ItemId, out var pList))
        //     {
        //         dto.PutawayRules = pList.Select(r =>
        //         {
        //             // default
        //             string targetName = r.TargetName;
        //             string targetCode = r.TargetCode;

        //             if (binLookup.TryGetValue(r.TargetId, out var bin))
        //             {
        //                 targetName = bin.BinName ?? targetName;
        //                 targetCode = bin.BinCode ?? targetCode;
        //             }
        //             else if (rackLookup.TryGetValue(r.TargetId, out var rack))
        //             {
        //                 targetName = rack.RackName ?? targetName;
        //                 targetCode = rack.RackCode ?? targetCode;
        //             }

        //             return new PutawayRuleDisplayDto
        //             {
        //                 WarehouseId = r.WarehouseId,
        //                 WarehouseCode = r.WarehouseId.HasValue && warehouseLookup.ContainsKey(r.WarehouseId.Value)
        //                     ? warehouseLookup[r.WarehouseId.Value].WarehouseCode
        //                     : null,
        //                 WarehouseName = r.WarehouseId.HasValue && warehouseLookup.ContainsKey(r.WarehouseId.Value)
        //                     ? warehouseLookup[r.WarehouseId.Value].WarehouseName
        //                     : null,

        //                 StorageTypeId = r.StorageTypeId,
        //                 StorageTypeName = r.StorageTypeName,
        //                 TargetId = r.TargetId,
        //                 TargetCode = targetCode,
        //                 TargetName = targetName,
        //                 PriorityId = r.PriorityId,
        //                 PriorityName = r.PriorityName
        //             };
        //         }).ToList();
        //     }
        // }


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