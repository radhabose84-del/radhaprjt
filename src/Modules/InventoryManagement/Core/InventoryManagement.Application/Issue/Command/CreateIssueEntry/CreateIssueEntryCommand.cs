using MediatR;

namespace InventoryManagement.Application.Issue.Command.CreateIssueEntry
{
    public class CreateIssueEntryCommand : IRequest<int>
    {
         public CreateIssueEntryDto IssueEntry { get; set; } = null!;
    }
}