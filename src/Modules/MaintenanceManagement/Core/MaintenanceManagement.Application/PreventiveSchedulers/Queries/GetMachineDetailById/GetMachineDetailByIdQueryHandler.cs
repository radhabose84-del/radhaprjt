// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
// using MediatR;

// namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
// {
//     public class GetMachineDetailByIdQueryHandler : IRequestHandler<GetMachineDetailByIdQuery, ApiResponseDTO<PreventiveSchedulerDto>>
//     {
//         private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
//          private readonly IMapper _mapper;
//          private readonly IDepartmentGrpcClient _departmentGrpcClient;
//         public GetMachineDetailByIdQueryHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper, IDepartmentGrpcClient departmentGrpcClient)
//         {
//             _preventiveSchedulerQuery = preventiveSchedulerQuery;
//             _mapper = mapper;
//             _departmentGrpcClient = departmentGrpcClient;
//         }
//         public async Task<ApiResponseDTO<PreventiveSchedulerDto>> Handle(GetMachineDetailByIdQuery request, CancellationToken cancellationToken)
//         {
//                var preventiveScheduler = await _preventiveSchedulerQuery.GetDetailSchedulerByPreventiveScheduleId(request.Id);
               
//             var preventiveSchedulerList = _mapper.Map<PreventiveSchedulerDto>(preventiveScheduler);
//             var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

              
//                    if (departmentLookup.TryGetValue(preventiveSchedulerList.DepartmentId, out var departmentName))
//                    {
//                        preventiveSchedulerList.DepartmentName = departmentName;
//                    }
               

//             //     var filteredPreventiveSchedulers = preventiveSchedulerList
//             // .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
//             // .ToList();
          
//             return new ApiResponseDTO<PreventiveSchedulerDto>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = preventiveSchedulerList
//             };
//         }
//     }
// }