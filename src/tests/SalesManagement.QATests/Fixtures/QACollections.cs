namespace SalesManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) because every collection shares the
// single `testsales` server session.

[CollectionDefinition("SalesOrganisationCollection")]
public sealed class SalesOrganisationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }

// ── Master entities ─────────────────────────────────────────────────────────
[CollectionDefinition("BusinessUnitCollection")]
public sealed class BusinessUnitCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesChannelCollection")]
public sealed class SalesChannelCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesSegmentCollection")]
public sealed class SalesSegmentCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesGroupCollection")]
public sealed class SalesGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesOfficeCollection")]
public sealed class SalesOfficeCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesContactCollection")]
public sealed class SalesContactCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DispatchAddressMasterCollection")]
public sealed class DispatchAddressMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DispatchAddressMappingCollection")]
public sealed class DispatchAddressMappingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ItemPriceMasterCollection")]
public sealed class ItemPriceMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesMiscMasterCollection")]
public sealed class SalesMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesMiscTypeMasterCollection")]
public sealed class SalesMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MovementTypeConfigCollection")]
public sealed class MovementTypeConfigCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesOrderTypeMasterCollection")]
public sealed class SalesOrderTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("StoTypeMasterCollection")]
public sealed class StoTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AgentCustomerMappingCollection")]
public sealed class AgentCustomerMappingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MarketingOfficerCollection")]
public sealed class MarketingOfficerCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("OfficerAgentCollection")]
public sealed class OfficerAgentCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DiscountMasterCollection")]
public sealed class DiscountMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AgentCommissionConfigCollection")]
public sealed class AgentCommissionConfigCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CommissionSplitCollection")]
public sealed class CommissionSplitCollection : ICollectionFixture<QAServerFixture> { }

// ── Transactional entities ──────────────────────────────────────────────────
[CollectionDefinition("ComplaintCollection")]
public sealed class ComplaintCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ComplaintQCReviewCollection")]
public sealed class ComplaintQCReviewCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ComplaintDepartmentFeedbackCollection")]
public sealed class ComplaintDepartmentFeedbackCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ComplaintResolutionCollection")]
public sealed class ComplaintResolutionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CustomerVisitCollection")]
public sealed class CustomerVisitCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DeliveryChallanCollection")]
public sealed class DeliveryChallanCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DispatchAdviceCollection")]
public sealed class DispatchAdviceCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("InvoiceCollection")]
public sealed class InvoiceCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProformaInvoiceCollection")]
public sealed class ProformaInvoiceCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesAgreementCollection")]
public sealed class SalesAgreementCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesEnquiryCollection")]
public sealed class SalesEnquiryCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesLeadCollection")]
public sealed class SalesLeadCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesOrderCollection")]
public sealed class SalesOrderCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesOrderAmendmentCollection")]
public sealed class SalesOrderAmendmentCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesQuotationCollection")]
public sealed class SalesQuotationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesQuotationAmendmentCollection")]
public sealed class SalesQuotationAmendmentCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesReturnCollection")]
public sealed class SalesReturnCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("StoHeaderCollection")]
public sealed class StoHeaderCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("StoReceiptCollection")]
public sealed class StoReceiptCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("TripSheetCollection")]
public sealed class TripSheetCollection : ICollectionFixture<QAServerFixture> { }

// ── Read-only / reporting entities ──────────────────────────────────────────
[CollectionDefinition("AgentPortalCollection")]
public sealed class AgentPortalCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LeadConversionFunnelCollection")]
public sealed class LeadConversionFunnelCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesProjectionCollection")]
public sealed class SalesProjectionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("StockLedgerCollection")]
public sealed class StockLedgerCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SalesAuditLogCollection")]
public sealed class SalesAuditLogCollection : ICollectionFixture<QAServerFixture> { }
