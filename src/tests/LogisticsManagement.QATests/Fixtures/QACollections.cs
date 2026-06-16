namespace LogisticsManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("FreightMasterCollection")]
public sealed class FreightMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LogisticsMiscMasterCollection")]
public sealed class LogisticsMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LogisticsMiscTypeMasterCollection")]
public sealed class LogisticsMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LogisticsAuditLogCollection")]
public sealed class LogisticsAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
