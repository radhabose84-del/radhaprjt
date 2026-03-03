using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class PackType : BaseEntity
    {
        public string? PackTypeCode { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public int? ConesPerBag { get; set; }
        public bool ProductionAllowed { get; set; } = true;
    }
}
