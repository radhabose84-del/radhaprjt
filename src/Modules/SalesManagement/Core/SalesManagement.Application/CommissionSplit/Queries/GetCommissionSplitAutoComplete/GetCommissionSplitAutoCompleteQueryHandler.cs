using AutoMapper;
using Contracts.Dtos.Lookups.Sales;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitAutoComplete
{
    public class GetCommissionSplitAutoCompleteQueryHandler : IRequestHandler<GetCommissionSplitAutoCompleteQuery, IReadOnlyList<CommissionSplitLookupDto>>
    {
        private readonly ICommissionSplitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCommissionSplitAutoCompleteQueryHandler(ICommissionSplitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<CommissionSplitLookupDto>> Handle(GetCommissionSplitAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCommissionSplitAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "CommissionSplit details was fetched.",
                module: "CommissionSplit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
