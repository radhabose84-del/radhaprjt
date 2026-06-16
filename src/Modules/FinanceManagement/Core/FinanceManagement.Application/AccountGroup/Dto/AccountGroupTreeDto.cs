namespace FinanceManagement.Application.AccountGroup.Dto
{
    public class AccountGroupTreeDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? AccountTypeId { get; set; }
        public int? ScheduleIIILineItemId { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public int? ParentAccountGroupId { get; set; }
        public int Level { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        // Accounts attach only at the leaf level.
        public bool IsLeaf { get; set; }
        public int ChildrenCount { get; set; }

        public List<AccountGroupTreeDto> Children { get; set; } = new();
    }
}
