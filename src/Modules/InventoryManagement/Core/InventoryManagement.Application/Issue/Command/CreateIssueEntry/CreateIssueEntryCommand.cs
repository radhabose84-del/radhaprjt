using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Issue.Command.CreateIssueEntry
{
    public class CreateIssueEntryCommand : IRequest<int>, IRequirePermission
    {
         public CreateIssueEntryDto IssueEntry { get; set; } = null!;
         public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
