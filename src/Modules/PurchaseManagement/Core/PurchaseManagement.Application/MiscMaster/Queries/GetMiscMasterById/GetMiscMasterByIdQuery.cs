using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace PurchaseManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
        
    }
}