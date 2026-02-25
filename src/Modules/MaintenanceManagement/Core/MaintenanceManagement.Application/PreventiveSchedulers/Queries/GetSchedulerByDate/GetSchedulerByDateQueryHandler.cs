#nullable disable
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetSchedulerByDate
{
    public class GetSchedulerByDateQueryHandler : IRequestHandler<GetSchedulerByDateQuery, ApiResponseDTO<List<SchedulerByDateDto>>>
    {
         private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
         private readonly IMapper _mapper;
         private readonly IDepartmentLookup _departmentLookup;

        public GetSchedulerByDateQueryHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper, IDepartmentLookup departmentLookup)
        {
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }
        public async Task<ApiResponseDTO<List<SchedulerByDateDto>>> Handle(GetSchedulerByDateQuery request, CancellationToken cancellationToken)
        {
            var preventiveScheduler = await _preventiveSchedulerQuery.GetAbstractSchedulerByDate(request.DepartmentId);
            
            var preventiveSchedulerList = _mapper.Map<List<SchedulerByDateDto>>(preventiveScheduler);
            
             var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                foreach (var dto in preventiveSchedulerList)
               {
                   if (departmentLookup.TryGetValue(dto.DepartmentId, out var departmentName))
                   {
                       dto.DepartmentName = departmentName;
                   }
               }

                var filteredPreventiveSchedulers = preventiveSchedulerList
            .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
            .ToList();

          
            return new ApiResponseDTO<List<SchedulerByDateDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = filteredPreventiveSchedulers
            };
        }
    }
}
