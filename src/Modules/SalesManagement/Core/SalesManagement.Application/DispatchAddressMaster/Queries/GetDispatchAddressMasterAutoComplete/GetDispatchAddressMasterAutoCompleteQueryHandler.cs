using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterAutoComplete
{
    public class GetDispatchAddressMasterAutoCompleteQueryHandler : IRequestHandler<GetDispatchAddressMasterAutoCompleteQuery, IReadOnlyList<DispatchAddressMasterLookupDto>>
    {
        private readonly IDispatchAddressMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAddressMasterAutoCompleteQueryHandler(IDispatchAddressMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DispatchAddressMasterLookupDto>> Handle(GetDispatchAddressMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<DispatchAddressMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDispatchAddressMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "DispatchAddressMaster details was fetched.",
                module: "DispatchAddressMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
