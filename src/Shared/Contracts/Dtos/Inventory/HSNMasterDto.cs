// Contracts.Dtos.Inventory/HSNMasterDto.cs
using System;

namespace Contracts.Dtos.Inventory
{
    public class HSNMasterDto
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; } = string.Empty;           // from ht.Code
        public string HSNCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int GSTCategoryId { get; set; }
        public string GstCategoryName { get; set; } = string.Empty;

        public decimal GSTPercentage { get; set; }
        public decimal CGSTPercentage { get; set; }
        public decimal SGSTPercentage { get; set; }
        public decimal IGSTPercentage { get; set; }

        public DateTimeOffset? ValidFrom { get; set; }
        public bool IsActive { get; set; }
    }
}
