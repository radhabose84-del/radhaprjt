using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveScheduler
{
    public class PreventiveSchedulerQueryHandler : IRequestHandler<GetPreventiveSchedulerQuery, ApiResponseDTO<List<GetPreventiveSchedulerDto>>>
    {
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public PreventiveSchedulerQueryHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup)
        {
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;

        }
        public async Task<ApiResponseDTO<List<GetPreventiveSchedulerDto>>> Handle(GetPreventiveSchedulerQuery request, CancellationToken cancellationToken)
        {
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentIds = departments.Select(d => d.DepartmentId).ToList();
            var (preventiveScheduler, totalCount) = await _preventiveSchedulerQuery.GetAllPreventiveSchedulerAsync(request.PageNumber, request.PageSize, request.SearchTerm,departmentIds);
            var preventiveSchedulerList = _mapper.Map<List<GetPreventiveSchedulerDto>>(preventiveScheduler);

            // 🔥 Fetch departments using gRPC
            

            // var departments = departmentResponse.Departments.ToList();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            // 🔥 Map department names with DataControl to preventiveScheduler
            // foreach (var data in preventiveSchedulerList)
            // {

            //     if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
            //     {
            //         data.DepartmentName = departmentName;
            //     }

            //     PreventiveSchedulerDictionary[data.DepartmentId] = data;

            // }
            foreach (var dto in preventiveSchedulerList)
        {
            if (departmentLookup.TryGetValue(dto.DepartmentId, out var departmentName))
            {
                dto.DepartmentName = departmentName;
            }
        }

                var filteredPreventiveSchedulers = preventiveSchedulerList
            .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
            .Where(p => departmentLookup.ContainsKey(p.ProductionDepartmentId))
            .ToList();
          

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPreventiveScheduler",
                actionCode: "",
                actionName: "",
                details: $"PreventiveScheduler details was fetched.",
                module: "PreventiveScheduler"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<GetPreventiveSchedulerDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = filteredPreventiveSchedulers,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
