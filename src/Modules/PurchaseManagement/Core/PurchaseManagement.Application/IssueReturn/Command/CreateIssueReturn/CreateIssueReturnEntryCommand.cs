using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
{
    public class CreateIssueReturnEntryCommand  : IRequest<int>, IRequirePermission
    {
        public CreateIssueReturnDto IssueReturnEntry { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
