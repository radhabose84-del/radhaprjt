<<<<<<< HEAD
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWarehouse;
// using Contracts.Common;
// using PurchaseManagement.Application.Common.Interfaces.IIssue;
// using PurchaseManagement.Domain.Events;
// using MediatR;
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IUser;
using Contracts.Interfaces.External.IWarehouse;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
>>>>>>> remotes/origin/ModulerMonolithic_DEV

namespace PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader
{
    public class GetPendingIssueHeaderQueryHandler : IRequestHandler<GetPendingIssueHeaderQuery, ApiResponseDTO<List<GetPendingIssueHeaderDto>>>
    {
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentAllLookup;
        private readonly IWarehouseLookup _warehouseLookup;

        public GetPendingIssueHeaderQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator, 
        IUnitLookup unitLookup, IDepartmentLookup departmentAllLookup, IWarehouseLookup warehouseLookup)
        {
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentAllLookup = departmentAllLookup;
            _warehouseLookup = warehouseLookup;
        }

        public async Task<ApiResponseDTO<List<GetPendingIssueHeaderDto>>> Handle(GetPendingIssueHeaderQuery request, CancellationToken cancellationToken)
        {
            var (result, totalCount) = await _iissueQueryCommandRepository.GetPendingIssueHeaderAsync(
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize,
            request.SearchTerm
        );

            var pendingGrnList = _mapper.Map<List<GetPendingIssueHeaderDto>>(result);
            var warehouseIds = result.Select(r => r.SubStoresWarehouseId)
                               .Where(id => id > 0)
                               .Distinct()
                               .ToList();
             // 3️⃣ Fire parallel gRPC calls
            var unitTask = _unitLookup.GetAllUnitAsync();
            var departmentTask = _departmentAllLookup.GetAllDepartmentAsync();
            var warehouseTask = _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken);
             // Wait for basics first
            await Task.WhenAll(unitTask, departmentTask, warehouseTask);
            var warehouseResults = await warehouseTask;
            // 4️⃣ Prepare lookup dictionaries
            var unitLookup = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
            var departmentLookup = (await departmentTask).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var warehouseLookup = warehouseResults
                .Where(w => w != null)
                .GroupBy(w => w.Id)
                .ToDictionary(g => g.Key, g => g.First().WarehouseName);


            // 5️⃣ Process and enrich each record
            foreach (var dto in pendingGrnList)
            {
                if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
                    dto.UnitName = unitName;

                if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.DepartmentName = deptName;

                if (departmentLookup.TryGetValue(dto.SubDepartmentId, out var subDeptName))
                    dto.SubDepartmentName = subDeptName;

                if (warehouseLookup.TryGetValue(dto.SubStoresWarehouseId, out var subWarehouseName))
                    dto.SubStoresWarehouseName = subWarehouseName;
            }

            // 5) Domain event (unchanged)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPendingIssueHeaderQuery",
                actionName: pendingGrnList.Count.ToString(),
                details: "Issue Pending details were fetched",
                module: "IssueEntry");

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GetPendingIssueHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = pendingGrnList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
