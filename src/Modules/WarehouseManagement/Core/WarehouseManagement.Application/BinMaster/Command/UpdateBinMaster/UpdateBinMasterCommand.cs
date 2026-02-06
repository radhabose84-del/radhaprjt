using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster
{
    public class UpdateBinMasterCommand : IRequest<int>
    {
         public int Id { get; set; }                 
        public string? BinName { get; set; }      
        public decimal BinCapacity { get; set; }    
        public int CapacityUOMId { get; set; }     
        public byte IsActive { get; set; }         
        public int? RackId { get; set; }
    }
}