using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class TransactionTypeMaster : BaseEntity
    {
        public int UnitId { get; set; }
        public int ModuleId { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
    }
}
