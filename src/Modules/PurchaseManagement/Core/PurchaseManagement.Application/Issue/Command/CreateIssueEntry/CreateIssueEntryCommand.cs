using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.Issue.Command.CreateIssueEntry
{
    public class CreateIssueEntryCommand : IRequest<int>
    {
        public CreateIssueEntryDto IssueEntry { get; set; } = null!;
    }
}