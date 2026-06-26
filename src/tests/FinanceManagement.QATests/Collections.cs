using Shared.QAInfrastructure.Fixtures;
using Xunit;

namespace FinanceManagement.QATests
{
    [CollectionDefinition("ScheduleIIICollection")]
    public sealed class ScheduleIIICollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("TaxCodeCollection")]
    public sealed class TaxCodeCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("CurrencyForexConfigCollection")]
    public sealed class CurrencyForexConfigCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("SecurityCollection")]
    public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("AccountGroupCollection")]
    public sealed class AccountGroupCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("CostCentreCollection")]
    public sealed class CostCentreCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("ProfitCentreCollection")]
    public sealed class ProfitCentreCollection : ICollectionFixture<QAServerFixture> { }

    // ── Added 2026-06-17: remaining Finance entities (extends the existing suite) ──
    [CollectionDefinition("FinMiscMasterCollection")]
    public sealed class FinMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("FinMiscTypeMasterCollection")]
    public sealed class FinMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("AccountTypeMasterCollection")]
    public sealed class AccountTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("VoucherTypeMasterCollection")]
    public sealed class VoucherTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("TransactionTypeMasterCollection")]
    public sealed class TransactionTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("DocumentSequenceCollection")]
    public sealed class DocumentSequenceCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("GlAccountMasterCollection")]
    public sealed class GlAccountMasterCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("CoaFreezeCollection")]
    public sealed class CoaFreezeCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("CoaChangeRequestCollection")]
    public sealed class CoaChangeRequestCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("GstrSectionMasterCollection")]
    public sealed class GstrSectionMasterCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("GstrSectionAccountLinkageCollection")]
    public sealed class GstrSectionAccountLinkageCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("TaxAccountLinkageCollection")]
    public sealed class TaxAccountLinkageCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("EInvoiceHeaderCollection")]
    public sealed class EInvoiceHeaderCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("EWaybillHeaderCollection")]
    public sealed class EWaybillHeaderCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("FinAuditLogCollection")]
    public sealed class FinAuditLogCollection : ICollectionFixture<QAServerFixture> { }

    // US-GL02-09 — Account Master Audit Trail & Version History (read-only viewer + export).
    [CollectionDefinition("AccountAuditTrailCollection")]
    public sealed class AccountAuditTrailCollection : ICollectionFixture<QAServerFixture> { }

    // US-GL03-01 — Financial Year & Period Calendar Setup.
    [CollectionDefinition("FinancialYearMasterCollection")]
    public sealed class FinancialYearMasterCollection : ICollectionFixture<QAServerFixture> { }

    // US-GL03-02 — Period status transitions (soft/hard close) + reversal override flow.
    [CollectionDefinition("FinancialPeriodStatusCollection")]
    public sealed class FinancialPeriodStatusCollection : ICollectionFixture<QAServerFixture> { }

    [CollectionDefinition("PeriodStatusOverrideCollection")]
    public sealed class PeriodStatusOverrideCollection : ICollectionFixture<QAServerFixture> { }

    // US-GL03-04 — Backdating controls + late-posting report.
    [CollectionDefinition("LatePostingReportCollection")]
    public sealed class LatePostingReportCollection : ICollectionFixture<QAServerFixture> { }

    // US-GL02-15 — COA Listing & Structure Reports (read-only + PDF export).
    [CollectionDefinition("CoaReportCollection")]
    public sealed class CoaReportCollection : ICollectionFixture<QAServerFixture> { }

    // US-GL02-16 — COA Read API for downstream modules (get-by-code / search / validate-for-posting).
    [CollectionDefinition("CoaReadCollection")]
    public sealed class CoaReadCollection : ICollectionFixture<QAServerFixture> { }
}
