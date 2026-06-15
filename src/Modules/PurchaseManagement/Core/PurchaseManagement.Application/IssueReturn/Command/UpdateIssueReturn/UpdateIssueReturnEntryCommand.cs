using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn
{
    public class UpdateIssueReturnEntryCommand : IRequest<bool>, IRequirePermission
    {
        public UpdateIssueReturnDto updateIssueReturnEntry { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
