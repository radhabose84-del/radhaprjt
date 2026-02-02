// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId
// {
//     public class GetPoServiceHeaderByPoIdQueryHandler : IRequestHandler<GetPoServiceHeaderByPoIdQuery, PoServiceHeaderByIdDto?>
//     {
       
//           private readonly  IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
//           private readonly IMapper _mapper;
//           private readonly IMediator _mediator;
//           private readonly IUnitGrpcClient _unitGrpcClient;
//           private readonly IPartyGrpcClient _partyGrpcClient;





//         public GetPoServiceHeaderByPoIdQueryHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator, IUnitGrpcClient unitGrpcClient, IPartyGrpcClient partyGrpcClient)
//         {
//             _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _unitGrpcClient = unitGrpcClient;
//             _partyGrpcClient = partyGrpcClient;

//         }
        
//         public async Task<PoServiceHeaderByIdDto?> Handle(GetPoServiceHeaderByPoIdQuery request, CancellationToken cancellationToken)
//         {

//               var units = await _unitGrpcClient.GetAllUnitAsync();
//               var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);      
          

//             // Fetch (repo may return multiple service headers for a single PO)
//             var entities = await _servicePurchaseOrderQueryRepository
//                 .GetServicePoHeaderByIdAsync(request.PoId, cancellationToken);

//                 if (entities is null)
//             return null;

//             // Map to DTOs (handle null from repo)
//             var headers = _mapper.Map<PoServiceHeaderByIdDto>(entities );

           
//                 if (headers.VendorId > 0)
//                 {
//                     var partyDetails = await _partyGrpcClient.GetPartyByIdAsync(headers.VendorId);
//                 if (partyDetails != null)
//                 {
//                     headers.VendorName = partyDetails.PartyName;
//                      headers.VendorCode = partyDetails.PartyCode;   
                                              
//                     }
//                 }
            

//               if (unitLookup.TryGetValue(headers.UnitId, out var unitName))
//                 headers.UnitName = unitName;

//             // Domain Event (log how many were fetched)
//             var count = headers;
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode: "GetPoServiceHeaderByPoIdQuery",
//                 actionName: $"Fetched {count} header(s)",
//                 details: $"Service PO header(s) fetched for PoId={request.PoId}.",
//                 module: "SES PO"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);

//             // You currently return only one item; keep that behavior:
//             return headers;
//         }

     
//     }
// }