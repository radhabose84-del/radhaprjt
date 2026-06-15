using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupAutoComplete
{
    public class GetAccountGroupAutoCompleteQueryHandler : IRequestHandler<GetAccountGroupAutoCompleteQuery, IReadOnlyList<AccountGroupLookupDto>>
    {
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAccountGroupAutoCompleteQueryHandler(
            IAccountGroupQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountGroupLookupDto>> Handle(GetAccountGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAccountGroupAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Account Group details was fetched.",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
