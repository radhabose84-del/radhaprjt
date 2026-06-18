namespace InventoryManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("InvMiscMasterCollection")]
public sealed class InvMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("InvMiscTypeMasterCollection")]
public sealed class InvMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("HSNMasterCollection")]
public sealed class HSNMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UOMCollection")]
public sealed class UOMCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UOMConversionCollection")]
public sealed class UOMConversionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UsageTypeCollection")]
public sealed class UsageTypeCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PriceGroupMasterCollection")]
public sealed class PriceGroupMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ItemSpecificationMasterCollection")]
public sealed class ItemSpecificationMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ItemSpecificationValueCollection")]
public sealed class ItemSpecificationValueCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ItemGroupCollection")]
public sealed class ItemGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ItemCategoryCollection")]
public sealed class ItemCategoryCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("InspectionTemplateCollection")]
public sealed class InspectionTemplateCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ItemMasterCollection")]
public sealed class ItemMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PutAwayRuleCollection")]
public sealed class PutAwayRuleCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("IssueEntryCollection")]
public sealed class IssueEntryCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MrsCollection")]
public sealed class MrsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("InvBudgetCollection")]
public sealed class InvBudgetCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("InvReportsCollection")]
public sealed class InvReportsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("InvAuditLogCollection")]
public sealed class InvAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
