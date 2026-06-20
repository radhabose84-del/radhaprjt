using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupApprovalChain
{
    // Read-only multilevel approval chain for the Move modal banner. Levels come from config
    // (Finance:MoveApprovalChain); the workflow engine performs the actual routing/enforcement.
    public sealed record GetAccountGroupApprovalChainQuery
        : IRequest<IReadOnlyList<AccountGroupApprovalChainDto>>;
}
