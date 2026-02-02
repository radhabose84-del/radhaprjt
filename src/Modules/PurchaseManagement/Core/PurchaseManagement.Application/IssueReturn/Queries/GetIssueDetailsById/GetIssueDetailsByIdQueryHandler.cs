using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IInvetoryManagement;
using Contracts.Interfaces.External.IUser;
using Contracts.Interfaces.External.IWarehouse;
using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssue;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById
{
    public class GetIssueDetailsByIdQueryHandler : IRequestHandler<GetIssueDetailsByIdQuery, List<GetIssueDetailsByIdDto>>
    {
          private readonly IIssueReturnEntryQueryRepository _issueReturnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        // private readonly IUnitGrpcClient _unitGrpcClient;
        // private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
        // private readonly IUOMGrpcClient _uOMGrpcClient;
        // private readonly IWarehouseGrpcClient _warehouseGrpcClient;
        // private readonly IRackGrpcClient _rackGrpcClient;
        // private readonly IBinGrpcClient _binGrpcClient;
        // private readonly IMiscMasterGrpcClient _miscMasterGrpcClient;
        // private readonly IInventoryGrpcClient _inventoryGrpcClient;
        public GetIssueDetailsByIdQueryHandler(IIssueReturnEntryQueryRepository issueReturnEntryQueryRepository, IMapper mapper, IMediator mediator
        // , IUnitGrpcClient unitGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient, IUOMGrpcClient uOMGrpcClient, IWarehouseGrpcClient warehouseGrpcClient, IRackGrpcClient rackGrpcClient, IBinGrpcClient binGrpcClient, IMiscMasterGrpcClient miscMasterGrpcClient, IInventoryGrpcClient inventoryGrpcClient
        )
        {
            _issueReturnEntryQueryRepository = issueReturnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            // _unitGrpcClient = unitGrpcClient;
            // _departmentAllGrpcClient = departmentAllGrpcClient;
            // _uOMGrpcClient = uOMGrpcClient;
            // _warehouseGrpcClient = warehouseGrpcClient;
            // _rackGrpcClient = rackGrpcClient;
            // _binGrpcClient = binGrpcClient;
            // _miscMasterGrpcClient = miscMasterGrpcClient;
            // _inventoryGrpcClient = inventoryGrpcClient;
        }


         public async Task<List<GetIssueDetailsByIdDto>> Handle(GetIssueDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            // 1️⃣ Fetch Issue details from repository
            var issueList = await _issueReturnEntryQueryRepository.GetIssueDetailsByIssueId(request.IssueHeaderId, request.ItemId);
            var issueReport = _mapper.Map<List<GetIssueDetailsByIdDto>>(issueList);

            if (issueReport == null || !issueReport.Any())
                return new List<GetIssueDetailsByIdDto>();

            // 2️⃣ Collect distinct lookup IDs
            var itemIds = issueReport
                .SelectMany(r => r.PendingIssueDetailsByIssueId.Select(d => d.ItemId))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var uomIds = issueReport
                .SelectMany(r => r.PendingIssueDetailsByIssueId.Select(d => d.UomId))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var warehouseIds = issueReport
                .SelectMany(r => r.PendingIssueDetailsByIssueId.Select(d => d.WarehouseStockId))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            // // 3️⃣ Fetch StorageType (Bin/Rack identifiers)
            // var storageTypes = await _miscMasterGrpcClient.GetMiscMasterByIdAsync("StorageType");

            // var binTypeIds = storageTypes
            //     .Where(x => string.Equals(x.Description, "Bin", StringComparison.OrdinalIgnoreCase))
            //     .Select(x => x.Id)
            //     .ToHashSet();

            // var rackTypeIds = storageTypes
            //     .Where(x => string.Equals(x.Description, "Rack", StringComparison.OrdinalIgnoreCase))
            //     .Select(x => x.Id)
            //     .ToHashSet();

            // // 4️⃣ Identify Bin & Rack Target IDs
            // var binIds = issueReport
            //     .SelectMany(r => r.PendingIssueDetailsByIssueId)
            //     .Where(d => binTypeIds.Contains(d.StorageTypeId))
            //     .Select(d => d.TargetId)
            //     .Where(id => id > 0)
            //     .Distinct()
            //     .ToList();

            // var rackIds = issueReport
            //     .SelectMany(r => r.PendingIssueDetailsByIssueId)
            //     .Where(d => rackTypeIds.Contains(d.StorageTypeId))
            //     .Select(d => d.TargetId)
            //     .Where(id => id > 0)
            //     .Distinct()
            //     .ToList();

            // // 5️⃣ Fire parallel lookups
            // var unitTask = _unitGrpcClient.GetAllUnitAsync();
            // var departmentTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
            // var toleranceTask = _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIds, cancellationToken);
            // var warehouseTasks = warehouseIds.Select(id => _warehouseGrpcClient.GetByIdAsync(id, cancellationToken)).ToList();
            // var binTasks = binIds.Select(id => _binGrpcClient.GetByIdAsync(id)).ToList();
            // var rackTasks = rackIds.Select(id => _rackGrpcClient.GetByIdAsync(id)).ToList();
            // var uomTasks = uomIds.Select(id => _uOMGrpcClient.GetByIdAsync(id, cancellationToken)).ToList();

            // await Task.WhenAll(
            //     toleranceTask,
            //     Task.WhenAll(warehouseTasks),
            //     Task.WhenAll(binTasks),
            //     Task.WhenAll(rackTasks),
            //     Task.WhenAll(uomTasks),
            //     unitTask,
            //     departmentTask
            // );

            // // 6️⃣ Build lookup dictionaries
            // var tolMap = toleranceTask.Result
            //     .GroupBy(t => t.ItemId)
            //     .ToDictionary(g => g.Key, g => g.First());

            // var warehouseById = warehouseTasks
            //     .Select(t => t.Result)
            //     .Where(x => x != null)
            //     .ToDictionary(x => x.Id, x => x.WarehouseName);

            // var binById = binTasks
            //     .Select(t => t.Result)
            //     .Where(x => x != null)
            //     .ToDictionary(x => x.Id, x => x);

            // var rackById = rackTasks
            //     .Select(t => t.Result)
            //     .Where(x => x != null)
            //     .ToDictionary(x => x.Id, x => x);

            // var uomById = uomTasks
            //     .Select(t => t.Result)
            //     .Where(x => x != null)
            //     .ToDictionary(x => x.Id, x => x);

            //     // 4️⃣ Prepare lookup dictionaries
            // var unitLookup = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
            // var departmentLookup = (await departmentTask).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            // // 7️⃣ Enrich DTOs
            // foreach (var issue in issueReport)
            // {
            //      if (unitLookup.TryGetValue(issue.UnitId, out var unitName))
            //         issue.UnitName = unitName;

            //     if (departmentLookup.TryGetValue(issue.DepartmentId, out var deptName))
            //         issue.DepartmentName = deptName;

            //     if (departmentLookup.TryGetValue(issue.SubDepartmentId, out var subDeptName))
            //         issue.SubDepartmentName = subDeptName;
                
                
            //     foreach (var detail in issue.PendingIssueDetailsByIssueId)
            //     {
            //         // 🧩 Item Name & Code
            //         if (tolMap.TryGetValue(detail.ItemId, out var tol))
            //         {
            //             detail.ItemName = tol.ItemName ?? "NA";
            //             detail.ItemCode = tol.ItemCode ?? "NA";
            //         }
            //         else
            //         {
            //             detail.ItemName = "NA";
            //             detail.ItemCode = "NA";
            //         }

            //         // 🏭 Warehouse Name
            //         if (warehouseById.TryGetValue(detail.WarehouseStockId, out var whName))
            //             detail.WarehouseStockName = whName;
            //         else
            //             detail.WarehouseStockName = "NA";

            //         // 🗄️ Storage Type Name
            //         if (storageTypes.FirstOrDefault(s => s.Id == detail.StorageTypeId) is { Description: var stName })
            //             detail.StorageTypeName = stName;
            //         else
            //             detail.StorageTypeName = "NA";

            //         // 📦 Bin / Rack details
            //         if (binTypeIds.Contains(detail.StorageTypeId) && binById.TryGetValue(detail.TargetId, out var bin))
            //         {
            //             detail.TargetName = bin.BinName ?? "NA";
            //             detail.TargetCode = bin.BinCode ?? "NA";
            //         }
            //         else if (rackTypeIds.Contains(detail.StorageTypeId) && rackById.TryGetValue(detail.TargetId, out var rack))
            //         {
            //             detail.TargetName = rack.RackName ?? "NA";
            //             detail.TargetCode = rack.RackCode ?? "NA";
            //         }
            //         else
            //         {
            //             detail.TargetName = "NA";
            //             detail.TargetCode = "NA";
            //         }

            //         // 📏 UOM Name
            //         if (uomById.TryGetValue(detail.UomId, out var uom))
            //             detail.UomName = uom.UOMName ?? "NA";
            //         else
            //             detail.UomName = "NA";
            //     }
            // }

            // 8️⃣ Domain Event Logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetIssueDetailsByIdQuery",
                actionName: issueReport.Count.ToString(),
                details: $"Issue details fetched for IssueId: {request.IssueHeaderId}",
                module: "Issue Return"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return issueReport;
        }

    }
}