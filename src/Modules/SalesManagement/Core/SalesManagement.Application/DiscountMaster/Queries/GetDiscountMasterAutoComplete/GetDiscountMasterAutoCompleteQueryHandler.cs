using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DiscountMaster.Queries.GetDiscountMasterAutoComplete
{
    public class GetDiscountMasterAutoCompleteQueryHandler : IRequestHandler<GetDiscountMasterAutoCompleteQuery, IReadOnlyList<DiscountMasterLookupDto>>
    {
        private readonly IDiscountMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDiscountMasterAutoCompleteQueryHandler(IDiscountMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DiscountMasterLookupDto>> Handle(GetDiscountMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDiscountMasterAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "DiscountMaster details was fetched.",
                module: "DiscountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
