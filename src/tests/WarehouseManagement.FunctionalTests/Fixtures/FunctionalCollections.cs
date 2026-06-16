namespace WarehouseManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-WH-01-WarehouseHierarchy")]
public sealed class WarehouseHierarchyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-WH-02-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }
