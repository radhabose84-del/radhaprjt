using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Queries.GetParentWarehouseMaster
{
    public class GetParentWarehouseMasterQueryHandler : IRequestHandler<GetParentWarehouseMasterQuery, List<GetParentWarehouseDto>>
    {

          private readonly IWarehouseMasterQueryRepository _warehouseMasterQueryRepository;
          
          public readonly IMapper _mapper;
          public readonly IMediator _mediator;

        public GetParentWarehouseMasterQueryHandler(IWarehouseMasterQueryRepository warehouseMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _warehouseMasterQueryRepository = warehouseMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }



        public async Task<List<GetParentWarehouseDto>> Handle(GetParentWarehouseMasterQuery request, CancellationToken cancellationToken)
        {


              var result = await _warehouseMasterQueryRepository.GetParentWarehouseMaster();
            
              var parentWarehouses = _mapper.Map<List<GetParentWarehouseDto>>(result);
       
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: " GetParentWarehouseMaster",
                actionName: parentWarehouses.Count.ToString(),
                details: $" Parent Warehouse details was fetched.",
                module: " Parent Warehouse"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return parentWarehouses;
           
        }
    }
}