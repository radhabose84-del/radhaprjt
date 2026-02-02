// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
// using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.ServiceMaster.Queries.GetServiceById
// {

//     public class GetServiceByIdQueryHandler : IRequestHandler< GetServiceByIdQuery,  GetServiceMasterDto>
//     {

//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         public readonly IServiceQueryRepository _serviceQueryRepository;
//          private readonly IUOMGrpcClient _uomGrpc;
//          private readonly IHSNGrpcClient _hsnGrpc;

//         public GetServiceByIdQueryHandler(IMapper mapper, IMediator mediator, IServiceQueryRepository serviceQueryRepository, IHSNGrpcClient hsnGrpc, IUOMGrpcClient uomGrpc)
//         {
//             _mapper = mapper;
//             _mediator = mediator;
//             _serviceQueryRepository = serviceQueryRepository;
//             _hsnGrpc = hsnGrpc;
//             _uomGrpc = uomGrpc;
//         }

//         public async Task<GetServiceMasterDto> Handle(GetServiceByIdQuery  request, CancellationToken cancellationToken)
//         {
//             var result = await _serviceQueryRepository.GetServiceMasterByIdAsync(request.Id);
//             if (result == null)
//             {
//                 throw new KeyNotFoundException($" Service with Id {request.Id} not found.");
//             }
//             var service = _mapper.Map<GetServiceMasterDto>(result);

//               var uomsTask = _uomGrpc.GetUOMAsync();
//             var hsnsTask = _hsnGrpc.GetAllAsync(PageNumber: 1, PageSize: 5000, SearchTerm: null);
//             await Task.WhenAll(uomsTask, hsnsTask);

//             var uoms = await uomsTask;                 // List<UomDto> with Id/Code/UOMName
//             var (hsnList, _) = await hsnsTask;         // (List<HSNMasterDto>, int)

//             var uom = uoms.FirstOrDefault(x => x.Id == service.UomId);
//             if (uom != null)
//                 service.UomName = uom.UOMName ?? uom.Code ?? service.UomName;

//             var hsn = hsnList.FirstOrDefault(x => x.Id == service.SacId);
//             if (hsn != null)
//                 service.SacName = hsn.Description ?? hsn.HSNCode ?? service.SacName;

//             var domainEvent = new AuditLogsDomainEvent(
//                actionDetail: "GetById",
//                actionCode: "GetServiceByIdQuery",
//                actionName: service.Id.ToString(),
//                details: $"Service {service.Id} was fetched.",
//                module: "Service"
//             );
//              await _mediator.Publish(domainEvent, cancellationToken);
//            return service;
           
//         }
//     }
// }