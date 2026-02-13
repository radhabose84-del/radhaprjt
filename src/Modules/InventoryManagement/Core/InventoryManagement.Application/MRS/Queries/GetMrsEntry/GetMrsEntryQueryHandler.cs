using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetMrsEntry
{
    public class GetMrsEntryQueryHandler : IRequestHandler<GetMrsEntryQuery, ApiResponseDTO<List<GetMrsEntryDto>>>
    {
         private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IWarehouseLookup _warehouseLookup;

        public GetMrsEntryQueryHandler(IMrsEntryQueryRepository iMrsEntryQueryRepository, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup, IWarehouseLookup warehouseLookup)
        {
            _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _warehouseLookup = warehouseLookup;
        }

        public async Task<ApiResponseDTO<List<GetMrsEntryDto>>> Handle(GetMrsEntryQuery request, CancellationToken cancellationToken)
        {
            var (result, totalCount) = await _iMrsEntryQueryRepository.GetMrsEntryDetails(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.FromDate,
            request.ToDate);

            var mrsEntryList = _mapper.Map<List<GetMrsEntryDto>>(result);
            var warehouseIds = result.Select(r => r.SubStoresWarehouseId)
                               .Where(id => id > 0)
                               .Distinct()
                               .ToList();
            var deptTask = _departmentLookup.GetAllDepartmentAsync();
            var warehouseTask = warehouseIds.Any()
                ? _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken)
                : Task.FromResult<IReadOnlyList<WarehouseLookupDto>>(Array.Empty<WarehouseLookupDto>());
              // Wait for basics first
            await Task.WhenAll(deptTask, warehouseTask);
            var departmentLookup = (await deptTask).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var warehouseLookup = (await warehouseTask)
                .Where(w => w != null)
                .ToDictionary(w => w.Id, w => w.WarehouseName);

               // 5️⃣ Process and enrich each record
            foreach (var dto in mrsEntryList)
            {

                if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.DepartmentName = deptName;

                if (departmentLookup.TryGetValue(dto.SubDepartmentId, out var subDeptName))
                    dto.SubDepartmentName = subDeptName;

                if (warehouseLookup.TryGetValue(dto.SubStoresWarehouseId, out var subWarehouseName))
                    dto.SubStoresWarehouseName = subWarehouseName;
            }

             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetMrsEntryQuery",        
                    actionName: mrsEntryList.Count.ToString(),
                    details: $"Mrs details was fetched.",
                    module:"MrsEntry"
                );
            await _mediator.Publish(domainEvent, cancellationToken);
              return new ApiResponseDTO<List<GetMrsEntryDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = mrsEntryList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
           
        }
    }

}
