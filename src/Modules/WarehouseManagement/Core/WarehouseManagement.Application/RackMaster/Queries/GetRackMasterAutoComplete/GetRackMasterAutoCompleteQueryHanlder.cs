using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete
{
    public class GetRackMasterAutoCompleteQueryHanlder : IRequestHandler<GetRackMasterAutoCompleteQuery, List<GetRackMasterAutoCompleteDto>>
    {
        private readonly IRackMasterQueryRepository _rackMasterQueryRepository;        
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public GetRackMasterAutoCompleteQueryHanlder(IRackMasterQueryRepository rackMasterQueryRepository, IMediator mediator, IMapper mapper)
        {
            _rackMasterQueryRepository = rackMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<GetRackMasterAutoCompleteDto>> Handle(GetRackMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var term = (request.SearchPattern ?? string.Empty).Trim();


            // Repo returns minimal fields for autocomplete (Id, RackCode, RackName)
            var result = await _rackMasterQueryRepository.GetRackMasterAutoCompletes(term ?? string.Empty, request.WarehouseId);

            // If your repo already returns DTOs, this map is effectively a no-op but harmless.
            var rackMasters = _mapper.Map<List<GetRackMasterAutoCompleteDto>>(result);

            // Domain Event (audit)
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