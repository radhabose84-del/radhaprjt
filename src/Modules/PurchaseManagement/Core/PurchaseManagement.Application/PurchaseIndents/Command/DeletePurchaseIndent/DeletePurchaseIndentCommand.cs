using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent
{
    public class DeletePurchaseIndentCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}