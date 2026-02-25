#nullable disable
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class GetMachineDetailByIdQueryHandler : IRequestHandler<GetMachineDetailByIdQuery, ApiResponseDTO<PreventiveSchedulerDto>>
    {
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
         private readonly IMapper _mapper;
         private readonly IDepartmentLookup _departmentLookup;
        public GetMachineDetailByIdQueryHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper, IDepartmentLookup departmentLookup)
        {
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }
        public async Task<ApiResponseDTO<PreventiveSchedulerDto>> Handle(GetMachineDetailByIdQuery request, CancellationToken cancellationToken)
        {
               var preventiveScheduler = await _preventiveSchedulerQuery.GetDetailSchedulerByPreventiveScheduleId(request.Id);
               
            var preventiveSchedulerList = _mapper.Map<PreventiveSchedulerDto>(preventiveScheduler);
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

              
                   if (departmentLookup.TryGetValue(preventiveSchedulerList.DepartmentId, out var departmentName))
                   {
                       preventiveSchedulerList.DepartmentName = departmentName;
                   }
               

            //     var filteredPreventiveSchedulers = preventiveSchedulerList
            // .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
            // .ToList();
          
            return new ApiResponseDTO<PreventiveSchedulerDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = preventiveSchedulerList
            };
        }
    }
}
