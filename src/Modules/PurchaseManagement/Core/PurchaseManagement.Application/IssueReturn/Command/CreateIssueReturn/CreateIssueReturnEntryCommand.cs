using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
{
    public class CreateIssueReturnEntryCommand  : IRequest<int>
    {
        public CreateIssueReturnDto IssueReturnEntry { get; set; } = null!;
    }
}