using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace InventoryManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery  :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
        
    }
}