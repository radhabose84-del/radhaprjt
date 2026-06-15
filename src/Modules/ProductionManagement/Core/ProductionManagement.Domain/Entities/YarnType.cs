using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class YarnType : BaseEntity
    {
        public string? YarnTypeCode { get; set; }
        public string? YarnTypeName { get; set; }
        public string? Description { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public int? CurrencyId { get; set; }
    }
}
