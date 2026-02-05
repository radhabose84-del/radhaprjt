using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster
{
    public class HSNMasterDto
    {
        public int Id { get; set; }

        public int TypeId { get; set; }
        public string? Type { get; set; }
        public string? HSNCode { get; set; }
        public string? Description { get; set; }
        public string? GstCategoryName { get; set; } // from MiscMaster
        public decimal GSTPercentage { get; set; }
        public decimal CGSTPercentage { get; set; }
        public decimal SGSTPercentage { get; set; }
        public decimal IGSTPercentage { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        
    }
}