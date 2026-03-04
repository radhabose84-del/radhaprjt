namespace SalesManagement.Application.MovementTypeConfig.Dto
{
    public class MovementTypeConfigDto
    {
        public int Id { get; set; }
        public string? MovementCode { get; set; }
        public string? MovementDescription { get; set; }

        public int MovementCategoryId { get; set; }
        public string? MovementCategoryName { get; set; }

        public int FromStockTypeId { get; set; }
        public string? FromStockTypeName { get; set; }

        public int ToStockTypeId { get; set; }
        public string? ToStockTypeName { get; set; }

        public bool QuantityUpdateFlag { get; set; }
        public bool ValueUpdateFlag { get; set; }
        public string? AccountModifier { get; set; }
        public bool BatchRequiredFlag { get; set; }
        public bool NegativeStockAllowed { get; set; }

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
