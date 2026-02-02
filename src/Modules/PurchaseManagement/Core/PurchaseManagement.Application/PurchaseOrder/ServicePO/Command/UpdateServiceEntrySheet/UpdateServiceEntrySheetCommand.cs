using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.UpdateServiceEntrySheet
{
    public class UpdateServiceEntrySheetCommand: IRequest<int>   {
        
        public int Id { get; set; }
        public CreateServiceSheetDto Data { get; set; } = null!;
    }
}