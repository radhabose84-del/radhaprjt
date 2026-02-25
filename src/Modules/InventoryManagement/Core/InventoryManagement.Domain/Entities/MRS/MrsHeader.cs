using InventoryManagement.Domain.Entities.Issue;

namespace InventoryManagement.Domain.Entities.MRS
{
    public class MrsHeader
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int RequestCategoryId { get; set; }
        public MiscMaster? StatusRequest { get; set; }
        public string? MrsNo { get; set; }
        public DateTimeOffset MrsDate { get; set; }
        public int DepartmentId { get; set; }
        public int SubDepartmentId { get; set; }
        public int? SubStoresWarehouseId { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ApprovedIP { get; set; }
        public string? Remarks { get; set; }
        public int StatusId { get; set; }
        public MiscMaster? StatusMrs { get; set; }
        public ICollection<MrsDetail>? MrsDetailHeaderName { get; set; }
        public ICollection<IssueHeader>? MrsIssueHeaderName { get; set; }
    }
}