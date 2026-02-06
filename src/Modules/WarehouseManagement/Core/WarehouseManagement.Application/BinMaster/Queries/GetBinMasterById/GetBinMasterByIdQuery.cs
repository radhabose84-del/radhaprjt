using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using MediatR;

namespace WarehouseManagement.Application.BinMaster.Queries.GetBinMasterById
{
    public class GetBinMasterByIdQuery : IRequest<BinMasterDto>
    {
        
         public int Id { get; set; }
        
    }
}