namespace GateEntryManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-GE-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-GE-02-GateMovement")]
public sealed class GateMovementCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-GE-03-AuditTrail")]
public sealed class AuditTrailCollection : ICollectionFixture<QAServerFixture> { }
