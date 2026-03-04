using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigAutoComplete
{
    public class GetMovementTypeConfigAutoCompleteQueryHandler : IRequestHandler<GetMovementTypeConfigAutoCompleteQuery, IReadOnlyList<MovementTypeConfigLookupDto>>
    {
        private readonly IMovementTypeConfigQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMovementTypeConfigAutoCompleteQueryHandler(
            IMovementTypeConfigQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<MovementTypeConfigLookupDto>> Handle(GetMovementTypeConfigAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<MovementTypeConfigLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetMovementTypeConfigAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "MovementTypeConfig details was fetched.",
                module: "MovementTypeConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
