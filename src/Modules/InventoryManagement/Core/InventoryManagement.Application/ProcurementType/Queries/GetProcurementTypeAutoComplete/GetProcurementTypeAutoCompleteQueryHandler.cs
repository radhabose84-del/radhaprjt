using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Application.ProcurementType.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Queries.GetProcurementTypeAutoComplete
{
    public class GetProcurementTypeAutoCompleteQueryHandler : IRequestHandler<GetProcurementTypeAutoCompleteQuery, IReadOnlyList<ProcurementTypeLookupDto>>
    {
        private readonly IProcurementTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProcurementTypeAutoCompleteQueryHandler(IProcurementTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ProcurementTypeLookupDto>> Handle(GetProcurementTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<ProcurementTypeLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetProcurementTypeAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "ProcurementType details was fetched.",
                module: "ProcurementType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
