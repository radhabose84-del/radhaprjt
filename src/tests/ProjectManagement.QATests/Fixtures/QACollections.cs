namespace ProjectManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("ProjectMasterCollection")]
public sealed class ProjectMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProjectWbsCollection")]
public sealed class ProjectWbsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProjectMiscMasterCollection")]
public sealed class ProjectMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProjectMiscTypeMasterCollection")]
public sealed class ProjectMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ProjectAuditLogCollection")]
public sealed class ProjectAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
