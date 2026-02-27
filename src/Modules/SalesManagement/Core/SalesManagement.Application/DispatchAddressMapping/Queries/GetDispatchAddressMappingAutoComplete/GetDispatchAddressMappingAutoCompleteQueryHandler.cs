using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingAutoComplete
{
    public class GetDispatchAddressMappingAutoCompleteQueryHandler : IRequestHandler<GetDispatchAddressMappingAutoCompleteQuery, IReadOnlyList<DispatchAddressMappingLookupDto>>
    {
        private readonly IDispatchAddressMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAddressMappingAutoCompleteQueryHandler(
            IDispatchAddressMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DispatchAddressMappingLookupDto>> Handle(GetDispatchAddressMappingAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var dtos = _mapper.Map<List<DispatchAddressMappingLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDispatchAddressMappingAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "DispatchAddressMapping details was fetched.",
                module: "DispatchAddressMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
