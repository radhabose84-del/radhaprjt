#nullable disable
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetDetailSchedulerByDate
{
    public class GetDetailSchedulerByDateQueryHandler : IRequestHandler<GetDetailSchedulerByDateQuery, ApiResponseDTO<List<DetailSchedulerByDateDto>>>
    {
         private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
         private readonly IMapper _mapper;
         private readonly IDepartmentLookup _departmentLookup;
        public GetDetailSchedulerByDateQueryHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper, IDepartmentLookup departmentLookup)
        {
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }
        public async Task<ApiResponseDTO<List<DetailSchedulerByDateDto>>> Handle(GetDetailSchedulerByDateQuery request, CancellationToken cancellationToken)
        {
             var preventiveScheduler = await _preventiveSchedulerQuery.GetDetailSchedulerByDate(request.SchedulerDate,request.DepartmentId);
            var preventiveSchedulerList = _mapper.Map<List<DetailSchedulerByDateDto>>(preventiveScheduler);
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
          
            return new ApiResponseDTO<List<DetailSchedulerByDateDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = filteredPreventiveSchedulers
            };
        }
    }
}
