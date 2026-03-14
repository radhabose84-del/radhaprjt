using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;
using WarehouseManagement.Domain.Events;

namespace WarehouseManagement.Application.WarehouseMaster.Queries.GetWarehouseByUnitId
{
    public class GetWarehouseByUnitIdQueryHandler : IRequestHandler<GetWarehouseByUnitIdQuery, List<GetWarehouseAutoCompleteDto>>
    {
        private readonly IWarehouseMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetWarehouseByUnitIdQueryHandler(
            IWarehouseMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<GetWarehouseAutoCompleteDto>> Handle(GetWarehouseByUnitIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByUnitIdAsync(request.UnitId);

            var dtos = _mapper.Map<List<GetWarehouseAutoCompleteDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByUnitId",
                actionCode: "GetWarehouseByUnitId",
                actionName: dtos.Count.ToString(),
                details: $"Warehouse list fetched by UnitId {request.UnitId}.",
                module: "WarehouseMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
