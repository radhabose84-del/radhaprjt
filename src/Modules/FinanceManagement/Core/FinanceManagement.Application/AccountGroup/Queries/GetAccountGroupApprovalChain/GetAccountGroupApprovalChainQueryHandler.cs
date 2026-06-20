using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupApprovalChain
{
    public class GetAccountGroupApprovalChainQueryHandler : IRequestHandler<GetAccountGroupApprovalChainQuery, IReadOnlyList<AccountGroupApprovalChainDto>>
    {
        // Used when Finance:MoveApprovalChain is absent from config.
        private static readonly IReadOnlyList<AccountGroupApprovalChainDto> DefaultChain = new List<AccountGroupApprovalChainDto>
        {
            new() { Level = 1, Label = "Finance Controller" },
            new() { Level = 2, Label = "CFO" }
        };

        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public GetAccountGroupApprovalChainQueryHandler(IConfiguration configuration, IMediator mediator)
        {
            _configuration = configuration;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountGroupApprovalChainDto>> Handle(GetAccountGroupApprovalChainQuery request, CancellationToken cancellationToken)
        {
            // Read the ordered chain from config (bound manually to avoid the Configuration.Binder package).
            var chain = _configuration.GetSection("Finance:MoveApprovalChain").GetChildren()
                .Select(c => new AccountGroupApprovalChainDto
                {
                    Level = int.TryParse(c["level"], out var lv) ? lv : 0,
                    Label = c["label"]
                })
                .Where(c => !string.IsNullOrWhiteSpace(c.Label))
                .OrderBy(c => c.Level)
                .ToList();

            if (chain.Count == 0)
                chain = DefaultChain.ToList();

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAccountGroupApprovalChainQuery",
                actionName: chain.Count.ToString(),
                details: "Account Group Move approval chain fetched (read-only banner).",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return chain;
        }
    }
}
