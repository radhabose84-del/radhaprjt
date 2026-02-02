using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.IssueReturn.Command.SaveIssueReturnStock
{
     public record SaveIssueReturnStockCommand(int IssueReturnHeaderId) : IRequest<bool>;
}