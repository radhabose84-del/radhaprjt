// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
// using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete
// {
//     public class GetMachineMasterAutoCompleteQueryHandler : IRequestHandler<GetMachineMasterAutoCompleteQuery,List<MachineMasterAutoCompleteDto>>
//     {
//         private readonly IMachineMasterQueryRepository _imachineMasterQueryRepository;        
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//          private readonly IDepartmentGrpcClient _departmentGrpcClient;

//         public GetMachineMasterAutoCompleteQueryHandler(IMachineMasterQueryRepository imachineMasterQueryRepository, IMapper mapper, IMediator mediator, IDepartmentGrpcClient departmentGrpcClient)
//         {
//             _imachineMasterQueryRepository = imachineMasterQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentGrpcClient = departmentGrpcClient;
//         }

//         public async Task<List<MachineMasterAutoCompleteDto>> Handle(GetMachineMasterAutoCompleteQuery request, CancellationToken cancellationToken)
//         {
//              var result = await _imachineMasterQueryRepository.GetMachineAsync(request.SearchPattern);
//             var machineMasters = _mapper.Map<List<MachineMasterAutoCompleteDto>>(result);
            
//               // 🔥 Fetch departments using gRPC
//             var departments = await _departmentGrpcClient.GetAllDepartmentAsync(); // ✅ Clean call
           
//             // 3. Create a list of valid DepartmentIds
//             var validDepartmentIds = departments.Select(d => d.DepartmentId).ToHashSet();

//             // 4. Filter only machineMasters whose DepartmentId exists in the gRPC department list
//             var filteredMachines = machineMasters
//                 .Where(m => validDepartmentIds.Contains(m.DepartmentId))
//                 .ToList();

               

//              //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                     actionDetail: "GetAll",
//                     actionCode: "GetMachineMasterAutoCompleteQuery",        
//                     actionName: filteredMachines.Count.ToString(),
//                     details: $"MachineMaster details was fetched.",
//                     module:"MachineMaster"
//                 );
//                 await _mediator.Publish(domainEvent, cancellationToken);
//             return filteredMachines;
//         }
//     }
// }