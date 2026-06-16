namespace ProjectManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-PRJ-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PRJ-02-ProjectLifecycle")]
public sealed class ProjectLifecycleCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PRJ-03-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }
