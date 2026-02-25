using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class IndentHeader : BaseEntity
    {
        public string IndentNumber { get; set; } = default!;
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public string Purpose { get; set; } = default!;
        public int DepartmentId { get; set; }
        public int StatusId { get; set; }
        public ICollection<IndentDetail> IndentDetails { get; set; } = default!;
        public MiscMaster IndentType { get; set; } = default!;
        public new MiscMaster Status { get; set; } = default!;
    }
}