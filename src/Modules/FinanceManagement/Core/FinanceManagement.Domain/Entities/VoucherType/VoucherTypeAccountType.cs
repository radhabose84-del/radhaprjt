using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-02 — allowed account types for a voucher type.
    // Same-module bridge to Finance.AccountTypeMaster (Asset/Liability/Income/Expense/Bank/Cash).
    public class VoucherTypeAccountType : BaseEntity
    {
        public int VoucherTypeId { get; set; }
        public int AccountTypeId { get; set; }

        // Same-module navigation
        public VoucherTypeMaster? VoucherType { get; set; }
        public AccountTypeMaster? AccountType { get; set; }
    }
}
