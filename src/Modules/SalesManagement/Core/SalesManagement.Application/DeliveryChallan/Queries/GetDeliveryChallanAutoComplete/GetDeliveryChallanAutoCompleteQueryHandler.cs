using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanAutoComplete
{
    public class GetDeliveryChallanAutoCompleteQueryHandler : IRequestHandler<GetDeliveryChallanAutoCompleteQuery, IReadOnlyList<DeliveryChallanLookupDto>>
    {
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDeliveryChallanAutoCompleteQueryHandler(
            IDeliveryChallanQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DeliveryChallanLookupDto>> Handle(GetDeliveryChallanAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDeliveryChallanAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "DeliveryChallan details was fetched.",
                module: "DeliveryChallan");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
