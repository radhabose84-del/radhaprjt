namespace BudgetManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("BudgetMiscMasterCollection")]
public sealed class BudgetMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BudgetMiscTypeMasterCollection")]
public sealed class BudgetMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BudgetGroupCollection")]
public sealed class BudgetGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BudgetAllocationCollection")]
public sealed class BudgetAllocationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BudgetRequestCollection")]
public sealed class BudgetRequestCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BudgetActivityLogsCollection")]
public sealed class BudgetActivityLogsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BudgetAuditLogCollection")]
public sealed class BudgetAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
