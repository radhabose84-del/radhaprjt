using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest
{
    public class GetMaintenanceRequestQueryHandler : IRequestHandler<GetMaintenanceRequestQuery, ApiResponseDTO<List<GetMaintenanceRequestDto>>>
    {
        private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUserLookup _userLookup;

        public GetMaintenanceRequestQueryHandler(
            IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUserLookup userLookup)
        {
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _userLookup = userLookup;
        }

        public async Task<ApiResponseDTO<List<GetMaintenanceRequestDto>>> Handle(GetMaintenanceRequestQuery request, CancellationToken cancellationToken)
        {
            var (maintenanceRequests, totalCount) = await _maintenanceRequestQueryRepository.GetAllMaintenanceRequestAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);
            var maintenanceRequestList = _mapper.Map<List<GetMaintenanceRequestDto>>(maintenanceRequests);

            // Fetch departments using lookup
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            // Fetch users using lookup
            var users = await _userLookup.GetAllUserAsync();
            var userLookup = users.ToDictionary(d => d.UserId, d => d.FirstName + " " + d.LastName);

            // 🔥 Map department names with DataControl to location

            foreach (var data in maintenanceRequestList)
            {

                if (departmentLookup.TryGetValue(data.ProductionDepartmentId, out var productionDepartmentName) && productionDepartmentName != null)
                {
                    data.ProductionDepartmentName = productionDepartmentName;
                }

                if (departmentLookup.TryGetValue(data.MaintenanceDepartmentId, out var maintenanceDepartmentName) && maintenanceDepartmentName != null)
                {
                    data.MaintenanceDepartmentName = maintenanceDepartmentName;
                }

                if (userLookup.TryGetValue(data.CreatedBy, out var userName) && userName != null)
                {
                    data.CreatedUsername = userName;
                 }
            }

            var filteredMaintenanceRequest = maintenanceRequestList
                .Where(p => departmentLookup.ContainsKey(p.ProductionDepartmentId))
                .ToList();
            
            // var filteredmaintenanceRequestDtos = maintenanceRequestList
            //  .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
            //  .Select(p => new GetMaintenanceRequestDto
            //  {
            //      DepartmentId = p.DepartmentId,
            //      DepartmentName = departmentLookup[p.DepartmentId],
            //  })
            //  .ToList();

            // Domain Event Logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "MaintenanceRequest records were fetched.",
                module: "MaintenanceRequest"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GetMaintenanceRequestDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = filteredMaintenanceRequest,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
