// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using WarehouseManagement.Application.Common.HttpResponse;
// using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
// using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
// using WarehouseManagement.Domain.Events;
// using MediatR;

// namespace WarehouseManagement.Application.RackMaster.Command.CreateRackMaster
// {
//     public class CreateRackMasterCommandHanlder : IRequestHandler<CreateRackMasterCommand, int>
//     {

//           private readonly IRackMasterCommandRepository  _rackMasterCommandRepository;
//         private readonly IRackCodeGenerator _rackCodeGenerator;
          
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;



//         public CreateRackMasterCommandHanlder(IRackMasterCommandRepository rackMasterCommandRepository, IMapper mapper, IMediator mediator, IRackCodeGenerator rackCodeGenerator)
//         {
//             _rackMasterCommandRepository = rackMasterCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _rackCodeGenerator = rackCodeGenerator;
//         }


//         //public async Task<int> Handle(CreateRackMasterCommand request, CancellationToken cancellationToken)
//         public async Task<int> Handle(CreateRackMasterCommand request, CancellationToken cancellationToken)    
//         {
//             var rackCode = await _rackCodeGenerator.GenerateAsync(
//                 request.WarehouseId, 
//                 request.FloorId,
//                 request.AisleId,
//                 request.RackLevelId
            
//             );                  
            
//             var rackMaster = _mapper.Map<WarehouseManagement.Domain.Entities.RackMaster>(request);
//               rackMaster.RackCode = rackCode;     

//             var newId = await _rackMasterCommandRepository.CreateAsync(rackMaster);
               
//                var auditEvent = new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode:   "RACK_CREATE",
//                 actionName:   rackMaster.RackCode,
//                 details:      $"RackMaster '{rackMaster.RackCode}' created successfully with Id {newId}.",
//                 module:       "RackMaster"
//             );
//             await _mediator.Publish(auditEvent, cancellationToken);

//             // Return just the new Id (since TResponse is int)
//             return newId;

//         }
//     }
// }