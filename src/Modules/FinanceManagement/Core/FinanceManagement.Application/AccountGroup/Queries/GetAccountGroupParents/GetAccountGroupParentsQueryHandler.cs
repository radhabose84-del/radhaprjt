using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupParents
{
    public class GetAccountGroupParentsQueryHandler : IRequestHandler<GetAccountGroupParentsQuery, IReadOnlyList<AccountGroupLookupDto>>
    {
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAccountGroupParentsQueryHandler(
            IAccountGroupQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountGroupLookupDto>> Handle(GetAccountGroupParentsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetParentsByLevelAsync(request.Level, request.CompanyId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAccountGroupParentsQuery",
                actionName: result.Count.ToString(),
                details: $"Eligible parent groups at level {request.Level} were fetched.",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
