namespace SalesManagement.FunctionalTests.Fixtures;

// One collection per workflow story. Each story is a single ordered test class that shares
// one QAServerFixture (one login, one run-unique EntityCode) across its steps. Keeping each
// story in its own collection lets stories run in parallel with each other while the steps
// WITHIN a story stay serialized via [TestPriority]. (Cross-collection parallelization is
// disabled in AssemblyInfo.cs because every collection shares the single `testsales` session.)

[CollectionDefinition("US-SALES-01-OrgSegmentSetup")]
public sealed class OrgSegmentSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-02-TerritorySetup")]
public sealed class TerritorySetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-03-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-04-MovementConfig")]
public sealed class MovementConfigCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-05-DiscountCommission")]
public sealed class DiscountCommissionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-06-MarketingAgents")]
public sealed class MarketingAgentsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-07-ItemPricing")]
public sealed class ItemPricingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-08-LeadToQuotation")]
public sealed class LeadToQuotationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-09-OrderLifecycle")]
public sealed class OrderLifecycleCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-10-DispatchInvoicing")]
public sealed class DispatchInvoicingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-11-StockTransfer")]
public sealed class StockTransferCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-12-ComplaintToReturn")]
public sealed class ComplaintToReturnCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-13-EngagementDashboards")]
public sealed class EngagementDashboardsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-SALES-14-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }
