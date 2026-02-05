// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IWarehouse;
// using InventoryManagement.Application.Common.Interfaces.IMRS;
// using InventoryManagement.Domain.Events;
// using MediatR;

// namespace InventoryManagement.Application.MRS.Queries.GetStockItemBased
// {
//     public class GetStockItemBasedQueryHandler : IRequestHandler<GetStockItemBasedQuery, List<GetStockItemDto>>
//     {
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IWarehouseGrpcClient _warehouseGrpcClient;
//         private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
//         public GetStockItemBasedQueryHandler(IMapper mapper, IMediator mediator, IWarehouseGrpcClient warehouseGrpcClient, IMrsEntryQueryRepository iMrsEntryQueryRepository)
//         {
//             _mapper = mapper;
//             _mediator = mediator;
//             _warehouseGrpcClient = warehouseGrpcClient;
//             _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
//         }

//         public async Task<List<GetStockItemDto>> Handle(GetStockItemBasedQuery request, CancellationToken cancellationToken)
//         {
//             // Fetch data from repository
//             var result = await _iMrsEntryQueryRepository.GetStockDetails(request.ItemId,request.WarehouseId);

//             // Map to DTOs (if needed — if repository already returns DTOs, you can skip this)
//             var getStockItems = _mapper.Map<List<GetStockItemDto>>(result);

//             // Domain Event logging
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "GetStockItemBasedQuery",
//                 actionName: getStockItems.Count.ToString(),
//                 details: $"Stock details was fetched.",
//                 module: "StockItem"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);

//             return getStockItems;
//         }
//     }
// }