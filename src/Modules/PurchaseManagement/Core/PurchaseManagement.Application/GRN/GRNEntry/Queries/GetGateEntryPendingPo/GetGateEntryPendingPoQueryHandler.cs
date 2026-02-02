// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IParty;
// using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo
// {
//     public class GetGateEntryPendingPoQueryHandler : IRequestHandler<GetGateEntryPendingPoQuery, List<GetGateEntryPendingPoDto>>
//     {
//         private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private IPartyGrpcClient _partyGrpcClient;

//         public GetGateEntryPendingPoQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IPartyGrpcClient partyGrpcClient)
//         {
//             _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _partyGrpcClient = partyGrpcClient;
//         }

//         public async Task<List<GetGateEntryPendingPoDto>> Handle(GetGateEntryPendingPoQuery request, CancellationToken cancellationToken)
//         {
//              var result = await _iGrnEntryQueryRepository.GetPendingPoAsync(request.PartyId);
//             var pendingpoIds = _mapper.Map<List<GetGateEntryPendingPoDto>>(result);
//              foreach (var po in pendingpoIds)
//             {
//                 if (po.PartyId > 0)
//                 {
//                     var partyDetails = await _partyGrpcClient.GetPartyByIdAsync(po.PartyId);
//                     if (partyDetails != null)
//                     {
//                         po.PartyName = partyDetails.PartyName;
                                              
//                     }
//                 }
//             }
            
//              //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                     actionDetail: "GetAll",
//                     actionCode: "GetGateEntryPendingPoQuery",        
//                     actionName: pendingpoIds.Count.ToString(),
//                     details: $"Pending PO details was fetched.",
//                     module:"GRNEntry"
//                 );
//                 await _mediator.Publish(domainEvent, cancellationToken);
//             return pendingpoIds;
//         }
//     }
// }