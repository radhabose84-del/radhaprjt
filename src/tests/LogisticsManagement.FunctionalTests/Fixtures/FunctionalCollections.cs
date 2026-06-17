namespace LogisticsManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-LOG-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-LOG-02-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }
