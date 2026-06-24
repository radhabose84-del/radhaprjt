namespace FinanceManagement.Application.AccountAuditTrail.Dto
{
    // One field-level audit row from Finance.AccountAuditTrail (US-GL02-09). Read-only.
    public sealed class AccountAuditTrailDto
    {
        public long Id { get; set; }
        public int CompanyId { get; set; }

        public string EntityName { get; set; } = null!;
        public int EntityId { get; set; }

        public string Action { get; set; } = null!;        // Insert / Update / Delete
        public string PropertyName { get; set; } = null!;  // changed field, or "*"
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedByRole { get; set; }
        public string? CreatedIP { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
