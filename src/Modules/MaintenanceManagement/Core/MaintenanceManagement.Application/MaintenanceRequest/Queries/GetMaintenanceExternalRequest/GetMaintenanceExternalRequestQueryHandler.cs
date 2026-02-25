using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceExternalRequest
{
    public class GetMaintenanceExternalRequestQueryHandler : IRequestHandler<GetMaintenanceExternalRequestQuery, ApiResponseDTO<List<GetMaintenanceExternalRequestDto>>>
    {

        private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public GetMaintenanceExternalRequestQueryHandler(
            IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup)
        {
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<List<GetMaintenanceExternalRequestDto>>> Handle(GetMaintenanceExternalRequestQuery request, CancellationToken cancellationToken)
        {
            var (maintenanceExternalRequests, totalCount) = await _maintenanceRequestQueryRepository.GetAllMaintenanceExternalRequestAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);
            var maintenanceRequestList = _mapper.Map<List<GetMaintenanceExternalRequestDto>>(maintenanceExternalRequests);

            // Fetch departments using lookup
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            foreach (var data in maintenanceRequestList)
            {

                if (departmentLookup.TryGetValue(data.ProductionDepartmentId, out var departmentName) && departmentName != null)
                {

                    data.ProductionDepartmentName = departmentName;
                }
                if (departmentLookup.TryGetValue(data.MaintenanceDepartmentId, out var maintenanceDepartmentName) && maintenanceDepartmentName != null)
                {

                    data.MaintenanceDepartmentName = maintenanceDepartmentName;
                }

            }

            var filteredMaintenanceRequest = maintenanceRequestList
                .Where(p => departmentLookup.ContainsKey(p.ProductionDepartmentId))
                .ToList();

            //    🔥 Map department names with DataControl
            // var filteredmaintenanceRequestDtos = maintenanceRequestList
            //     .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
            //     .Select(p => new GetMaintenanceExternalRequestDto
            //     {
            //         DepartmentId = p.DepartmentId,
            //         DepartmentName = departmentLookup[p.DepartmentId],
            //     })
            //     .ToList();

            // Domain Event Logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "MaintenanceRequest records were fetched.",
                module: "MaintenanceRequest"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GetMaintenanceExternalRequestDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = filteredMaintenanceRequest,
                TotalCount =  totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }


    }
}
