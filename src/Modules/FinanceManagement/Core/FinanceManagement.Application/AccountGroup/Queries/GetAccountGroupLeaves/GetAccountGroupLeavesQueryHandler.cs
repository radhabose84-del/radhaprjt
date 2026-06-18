using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupLeaves
{
    public class GetAccountGroupLeavesQueryHandler : IRequestHandler<GetAccountGroupLeavesQuery, IReadOnlyList<AccountGroupLookupDto>>
    {
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAccountGroupLeavesQueryHandler(
            IAccountGroupQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountGroupLookupDto>> Handle(GetAccountGroupLeavesQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.GetLeafGroupsAsync(companyId, request.AccountTypeId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAccountGroupLeavesQuery",
                actionName: result.Count.ToString(),
                details: $"Assignable leaf groups fetched (company {companyId}, accountType {request.AccountTypeId}).",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
