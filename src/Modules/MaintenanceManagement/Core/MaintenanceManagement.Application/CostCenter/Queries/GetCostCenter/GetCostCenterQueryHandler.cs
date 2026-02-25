using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter
{
    public class GetCostCenterQueryHandler : IRequestHandler<GetCostCenterQuery, ApiResponseDTO<List<CostCenterDto>>>
    {
        private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;              // ✅ Unit lookup
        private readonly IDepartmentLookup _departmentLookup;  // ✅ Department lookup


        public GetCostCenterQueryHandler(ICostCenterQueryRepository iCostCenterQueryRepository, IMapper mapper, IMediator mediator
            , IUnitLookup unitLookup,                 // ✅ inject
            IDepartmentLookup departmentLookup)      // ✅ injec
        {
            _iCostCenterQueryRepository = iCostCenterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;

        }

        public async Task<ApiResponseDTO<List<CostCenterDto>>> Handle(GetCostCenterQuery request, CancellationToken cancellationToken)
        {
            var (list, totalCount) =
                await _iCostCenterQueryRepository.GetAllCostCenterListGroupAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm);

            list ??= new List<CostCenterDto>();

            // ✅ Enrich UnitName + DepartmentName using lookups (UserManagement owns the data)
            if (list.Count > 0)
            {
                // Units
                var unitIds = list
                    .Select(x => x.UnitId)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToArray();

                if (unitIds.Length > 0)
                {
                    var units = await _unitLookup.GetByIdsAsync(unitIds, cancellationToken);
                    var unitMap = units
                        .Where(u => u != null)
                        .ToDictionary(u => u.UnitId, u => u.UnitName);

                    foreach (var row in list)
                    {
                        if (unitMap.TryGetValue(row.UnitId, out var unitName))
                            row.UnitName = unitName;
                    }
                }

                // Departments
                var deptIds = list
                    .Select(x => x.DepartmentId)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToArray();

                if (deptIds.Length > 0)
                {
                    var depts = await _departmentLookup.GetByIdsAsync(deptIds, cancellationToken);
                    var deptMap = depts
                        .Where(d => d != null)
                        .ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                    foreach (var row in list)
                    {
                        if (deptMap.TryGetValue(row.DepartmentId, out var deptName))
                            row.DepartmentName = deptName;
                    }
                }
            }

            // var (list, totalCount) =
            //     await _iCostCenterQueryRepository.GetAllCostCenterListGroupAsync(
            //         request.PageNumber,
            //         request.PageSize,
            //         request.SearchTerm);

            // list ??= new List<CostCenterDto>();

            // 📘 Log domain event
            await _mediator.Publish(new AuditLogsDomainEvent(
             actionDetail: "GetCostCenter",
             actionCode: "Get",
             actionName: list.Count.ToString(),
             details: "CostCenter details were fetched.",
             module: "CostCenter"
         ), cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<CostCenterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                // Data = costCenterDtos,
                Data = list,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }



    }
}