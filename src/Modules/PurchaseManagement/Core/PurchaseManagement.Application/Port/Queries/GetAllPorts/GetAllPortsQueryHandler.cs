// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
// using PurchaseManagement.Application.Port.Dto;
// using PurchaseManagement.Application.Port.Queries.GetAllPorts;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.Purchase.PortMaster.Handlers;

// public sealed class GetAllPortsQueryHandler : IRequestHandler<GetAllPortsQuery, PagedResult<PortMasterDto>>
// {
//     private readonly IPortMasterQueryRepository _repo;
//     private readonly IMapper _mapper;
//     private readonly IMediator _mediator;
//     private readonly ICountryGrpcClient _countryGrpcClient;
//     public GetAllPortsQueryHandler( IPortMasterQueryRepository repo,
//             IMapper mapper,
//             IMediator mediator,
//             ICountryGrpcClient countryGrpcClient)
//     {
//         _repo = repo;
//         _mapper = mapper;
//         _mediator = mediator;
//         _countryGrpcClient = countryGrpcClient;
//     }       

//     public async Task<PagedResult<PortMasterDto>> Handle(GetAllPortsQuery request, CancellationToken ct)
//     {
//        var (rows, total) = await _repo.GetAllAsync(
//                 page: request.PageNumber,
//                 size: request.PageSize,
//                 search: request.Search,
//                 countryId: request.CountryId,                
//                 portTypeId: request.PortTypeId,
//                 ct
//             );

//             var ports = _mapper.Map<List<PortMasterDto>>(rows);            
//             var countriesTask = _countryGrpcClient.GetAllCountryAsync();             
//             await Task.WhenAll(countriesTask );

//             var countryDict = countriesTask.Result
//                 .GroupBy(x => x.CountryId)
//                 .ToDictionary(g => g.Key, g => g.First().CountryName);

   
//             // 3) enrich results
//             foreach (var p in ports)
//             {
//                 // Country name
//                 if (countryDict.TryGetValue(p.CountryId, out var cname))
//                     p.Country = cname;              
//             }

//             // 4) audit
//             var ev = new AuditLogsDomainEvent(
//                 actionDetail: "GetPortsQuery",
//                 actionCode: "Get",
//                 actionName: ports.Count.ToString(),
//                 details: "Port master list fetched.",
//                 module: "PortMaster"
//             );
//             await _mediator.Publish(ev, ct);

//             // 5) response
//            return new PagedResult<PortMasterDto> { Items = rows, Total = total };
          
//     }
// }
