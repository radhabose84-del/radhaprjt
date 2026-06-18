namespace ProductionManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-PROD-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PROD-02-LotAndProduction")]
public sealed class LotAndProductionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PROD-03-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }
