namespace FinanceManagement.Application.AccountGroup.Dto
{
    public class AccountGroupDetailDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }
        public int? ScheduleIIISectionItemId { get; set; }
        public string? ScheduleIIILineName { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public int? ParentAccountGroupId { get; set; }
        public string? ParentGroupName { get; set; }
        public int Level { get; set; }
        public int SortOrder { get; set; }
        public bool IsLeaf { get; set; }

        // Breadcrumb path, e.g. "Assets → Current Assets → Inventories → Finished Goods — Fabric".
        public string? Path { get; set; }
        public int ChildrenCount { get; set; }

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
