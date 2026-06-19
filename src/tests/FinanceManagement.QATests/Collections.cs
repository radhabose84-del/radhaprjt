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
}
