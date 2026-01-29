// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
// using MediatR;

// namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetSchedulerByDate
// {
//     public class GetSchedulerByDateQueryHandler : IRequestHandler<GetSchedulerByDateQuery, ApiResponseDTO<List<SchedulerByDateDto>>>
//     {
//          private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
//          private readonly IMapper _mapper;
//          private readonly IDepartmentGrpcClient _departmentGrpcClient;

//         public GetSchedulerByDateQueryHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper, IDepartmentGrpcClient departmentGrpcClient)
//         {
//             _preventiveSchedulerQuery = preventiveSchedulerQuery;
//             _mapper = mapper;
//             _departmentGrpcClient = departmentGrpcClient;
//         }
//         public async Task<ApiResponseDTO<List<SchedulerByDateDto>>> Handle(GetSchedulerByDateQuery request, CancellationToken cancellationToken)
//         {
//             var preventiveScheduler = await _preventiveSchedulerQuery.GetAbstractSchedulerByDate(request.DepartmentId);
            
//             var preventiveSchedulerList = _mapper.Map<List<SchedulerByDateDto>>(preventiveScheduler);
            
//              var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//                 foreach (var dto in preventiveSchedulerList)
//                {
//                    if (departmentLookup.TryGetValue(dto.DepartmentId, out var departmentName))
//                    {
//                        dto.DepartmentName = departmentName;
//                    }
//                }

//                 var filteredPreventiveSchedulers = preventiveSchedulerList
//             .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
//             .ToList();

          
//             return new ApiResponseDTO<List<SchedulerByDateDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = filteredPreventiveSchedulers
//             };
//         }
//     }
// }