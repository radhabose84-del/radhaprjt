using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete
{
    public class GetWarehouseMasterAutoCompleteQueryHandler : IRequestHandler<GetWarehouseMasterAutoCompleteQuery, List<GetWarehouseAutoCompleteDto>>
    {        

         private readonly  IWarehouseMasterQueryRepository _warehouseMasterQueryRepository;
          private readonly IMediator _mediator;
         private readonly IMapper _mapper;

        public GetWarehouseMasterAutoCompleteQueryHandler(IWarehouseMasterQueryRepository warehouseMasterQueryRepository, IMediator mediator, IMapper mapper)
        {
            _warehouseMasterQueryRepository = warehouseMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<GetWarehouseAutoCompleteDto>> Handle(GetWarehouseMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
              var term = (request.SearchPattern ?? string.Empty).Trim();
           
            var result = await _warehouseMasterQueryRepository.GetWarehouseMasterAutoCompletes(term);
           
            var rackMasters = _mapper.Map<List<GetWarehouseAutoCompleteDto>>(result);
       
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetRackMasterAutoComplete",
                actionName: rackMasters.Count.ToString(),
                details: $"RackMaster autocomplete fetched for term '{term}'.",
                module: "RackMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return rackMasters;
        }
    }
}