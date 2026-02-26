using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesChannel.Queries.GetSalesChannelAutoComplete
{
    public class GetSalesChannelAutoCompleteQueryHandler : IRequestHandler<GetSalesChannelAutoCompleteQuery, IReadOnlyList<SalesChannelLookupDto>>
    {
        private readonly ISalesChannelQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesChannelAutoCompleteQueryHandler(ISalesChannelQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesChannelLookupDto>> Handle(GetSalesChannelAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var salesChannels = _mapper.Map<List<SalesChannelLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesChannelAutoCompleteQuery",
                actionName: salesChannels.Count.ToString(),
                details: "SalesChannel details was fetched.",
                module: "SalesChannel"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesChannels;
        }
    }
}



