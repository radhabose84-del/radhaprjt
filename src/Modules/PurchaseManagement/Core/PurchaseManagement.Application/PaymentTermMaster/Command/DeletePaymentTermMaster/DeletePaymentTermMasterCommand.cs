using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster
{
    public class DeletePaymentTermMasterCommand: IRequest<bool>
    {
        public int Id { get; init; }
        
    }
}