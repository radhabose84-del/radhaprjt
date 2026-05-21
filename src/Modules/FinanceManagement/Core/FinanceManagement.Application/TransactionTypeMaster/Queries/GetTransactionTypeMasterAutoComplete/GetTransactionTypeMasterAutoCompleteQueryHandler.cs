using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterAutoComplete
{
    public class GetTransactionTypeMasterAutoCompleteQueryHandler : IRequestHandler<GetTransactionTypeMasterAutoCompleteQuery, IReadOnlyList<TransactionTypeMasterLookupDto>>
    {
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTransactionTypeMasterAutoCompleteQueryHandler(
            ITransactionTypeMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<TransactionTypeMasterLookupDto>> Handle(GetTransactionTypeMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, request.MenuId, cancellationToken);
            var dtos = _mapper.Map<List<TransactionTypeMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetTransactionTypeMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Transaction Type Master details was fetched.",
                module: "TransactionTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
