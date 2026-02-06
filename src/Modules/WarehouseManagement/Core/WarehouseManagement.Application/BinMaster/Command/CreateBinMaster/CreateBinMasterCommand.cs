using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace WarehouseManagement.Application.BinMaster.Command.CreateBinMaster
{
    public class CreateBinMasterCommand    :   IRequest<int>
    {
     //   public string BinCode { get; set; } = string.Empty;
        public string BinName { get; set; } = string.Empty;

        public int WarehouseId { get; set; }

        public int? RackId { get; set; }

        public decimal BinCapacity { get; set; }
         
        public int CapacityUOMId { get; set; }
    }
}