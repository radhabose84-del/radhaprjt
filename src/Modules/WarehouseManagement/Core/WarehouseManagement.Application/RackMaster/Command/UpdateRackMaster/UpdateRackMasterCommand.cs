using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster
{
    public class UpdateRackMasterCommand  : IRequest<int>
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string? RackName { get; set; }
        public int? FloorId { get; set; }
        public int? AisleId { get; set; }
        public int? RackLevelId { get; set; }
        public decimal? MaxCapacity { get; set; }
        public int? CapacityUOMId { get; set; }
        public decimal? RackWidth { get; set; }
        public decimal? RackHeight { get; set; }
        public int? DimensionUOMId { get; set; }        
        public byte IsActive { get; set; }
        
    }
}