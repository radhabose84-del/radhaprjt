using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion
{
    public class UOMConversionDto  
    {
         public int Id { get; set; }
        public int FromUOMId { get; set; }
        public string FromUOMCode { get; set; } = string.Empty;
        public int ToUOMId { get; set; }
        public string ToUOMCode { get; set; } = string.Empty;
        public decimal ConversionValue { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string CreatedIP { get; set; } = string.Empty;
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}