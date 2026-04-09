namespace SalesManagement.Application.CommissionSplit.Dto
{
    public class CommissionSplitDto
    {
        public int Id { get; set; }
        public string? SplitCode { get; set; }
        public string? SplitName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Child collection
        public List<CommissionSplitDetailDto>? Details { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }

    public class CommissionSplitDetailDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int ShareTypeId { get; set; }
        public string? ShareTypeName { get; set; }
        public decimal ShareValue { get; set; }
    }
}
