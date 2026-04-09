using MediatR;

namespace SalesManagement.Application.CommissionSplit.Commands.DeleteCommissionSplit
{
    public sealed record DeleteCommissionSplitCommand(int Id) : IRequest<bool>;
}
