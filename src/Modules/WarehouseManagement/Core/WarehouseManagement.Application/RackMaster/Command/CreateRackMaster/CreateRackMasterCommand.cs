using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Command.CreateRackMaster
{
    public class CreateRackMasterCommand :   IRequest<int>
    {
        
        public int WarehouseId { get; set; }         
       /// public string RackCode { get; set; } = default!;    
        public string? RackName { get; set; }
        public int? FloorId { get; set; }        
        public int? AisleId { get; set; }        
        public int? RackLevelId { get; set; }        
        public decimal? MaxCapacity { get; set; }
        public int? CapacityUOMId { get; set; }        
        public decimal? RackWidth { get; set; }
        public decimal? RackHeight { get; set; }
        public int? DimensionUOMId { get; set; }
        
        
    }
}