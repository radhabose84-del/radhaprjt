using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Command.SaveIssueReturnStock
{
    public record SaveIssueReturnStockCommand(int IssueReturnHeaderId) : IRequest<bool>;
}