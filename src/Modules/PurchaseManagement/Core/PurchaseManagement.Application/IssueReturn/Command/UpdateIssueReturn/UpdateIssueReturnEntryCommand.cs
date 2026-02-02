using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn
{
    public class UpdateIssueReturnEntryCommand : IRequest<bool>
    {
        public UpdateIssueReturnDto updateIssueReturnEntry { get; set; } = null!;
    }
}