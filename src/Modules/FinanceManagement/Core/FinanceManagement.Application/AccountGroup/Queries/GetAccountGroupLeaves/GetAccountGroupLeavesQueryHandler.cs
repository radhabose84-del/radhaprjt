using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupLeaves
{
    public class GetAccountGroupLeavesQueryHandler : IRequestHandler<GetAccountGroupLeavesQuery, IReadOnlyList<AccountGroupLookupDto>>
    {
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAccountGroupLeavesQueryHandler(
            IAccountGroupQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountGroupLookupDto>> Handle(GetAccountGroupLeavesQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetLeafGroupsAsync(request.CompanyId, request.AccountTypeId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAccountGroupLeavesQuery",
                actionName: result.Count.ToString(),
                details: $"Assignable leaf groups fetched (company {request.CompanyId}, accountType {request.AccountTypeId}).",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
