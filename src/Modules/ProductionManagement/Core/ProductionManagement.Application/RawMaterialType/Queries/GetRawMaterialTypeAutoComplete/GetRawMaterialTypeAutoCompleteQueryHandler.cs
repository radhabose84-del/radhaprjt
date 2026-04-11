using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeAutoComplete
{
    public class GetRawMaterialTypeAutoCompleteQueryHandler : IRequestHandler<GetRawMaterialTypeAutoCompleteQuery, IReadOnlyList<RawMaterialTypeLookupDto>>
    {
        private readonly IRawMaterialTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRawMaterialTypeAutoCompleteQueryHandler(
            IRawMaterialTypeQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<RawMaterialTypeLookupDto>> Handle(GetRawMaterialTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<RawMaterialTypeLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetRawMaterialTypeAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Raw Material Type details was fetched.",
                module: "RawMaterialType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
